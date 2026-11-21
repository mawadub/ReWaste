using System.Net;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Tests.Integration;

public class SavedResponseTests : IClassFixture<ReWasteFactory>
{
    private readonly ReWasteFactory _factory;
    public SavedResponseTests(ReWasteFactory factory) => _factory = factory;

    // Puts a saved result straight into the database for the given user (arrange step).
    private async Task<int> SeedSavedResponseAsync(string userId, string brand = "Apple")
    {
        return await _factory.WithDbAsync(async db =>
        {
            var row = new SavedResponse
            {
                Brand = brand,
                Model = "iPhone 6s",
                AgeInYears = 2,
                Condition = Condition.Working,
                PostcodeDistrict = "EH1",
                Route1 = RouteType.Donate,
                Reason1 = "Seeded reason",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };
            db.SavedResponses.Add(row);
            await db.SaveChangesAsync();
            return row.Id;
        });
    }

    [Fact] // IT-20
    public async Task Anonymous_Save_Is_Redirected_To_Login_And_Saves_Nothing()
    {
        var client = _factory.CreateBrowserClient();
        var before = await _factory.WithDbAsync(db => db.SavedResponses.CountAsync());

        // Token from any page with a form; [Authorize] runs before the anti-forgery check.
        var response = await Web.PostFormAsync(client, "/Identity/Account/Login", "/Wizard/Save", new());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Identity/Account/Login", Web.RedirectPath(response));
        var after = await _factory.WithDbAsync(db => db.SavedResponses.CountAsync());
        Assert.Equal(before, after);
    }

    [Fact] // IT-21: FR 11 / FR 27: the saved row belongs to the logged-in user
    public async Task Save_Stores_A_Row_Owned_By_The_Logged_In_User()
    {
        var user = await _factory.CreateTestUserAsync();
        var client = _factory.CreateBrowserClient();
        await Web.LoginAsync(client, user.Email!, ReWasteFactory.TestUserPassword);
        await Web.CompleteWizardAsync(client, brand: "Apple", model: "iPhone 6s", postcode: "EH1 2NG");

        var save = await Web.PostFormAsync(client, "/Wizard/Results", "/Wizard/Save", new());

        Assert.Equal(HttpStatusCode.Redirect, save.StatusCode);
        Assert.Equal("/MyDevices", Web.RedirectPath(save));

        var row = await _factory.WithDbAsync(db => db.SavedResponses.SingleAsync(s => s.UserId == user.Id));
        Assert.Equal("Apple", row.Brand);
        Assert.Equal("iPhone 6s", row.Model);
        Assert.Equal("EH1", row.PostcodeDistrict); // privacy: only the district, never the full postcode
    }

    [Fact] // IT-22: NFR 11
    public async Task User_Only_Sees_Their_Own_Saved_Results_In_The_List()
    {
        var a = await _factory.CreateTestUserAsync();
        var b = await _factory.CreateTestUserAsync();
        await SeedSavedResponseAsync(a.Id, brand: "BrandOfUserA");
        await SeedSavedResponseAsync(b.Id, brand: "BrandOfUserB");

        var client = _factory.CreateBrowserClient();
        await Web.LoginAsync(client, a.Email!, ReWasteFactory.TestUserPassword);

        var text = await Web.TextAsync(await client.GetAsync("/MyDevices"));

        Assert.Contains("BrandOfUserA", text);
        Assert.DoesNotContain("BrandOfUserB", text);
    }

    [Fact] // IT-23: NFR 11: another user's id behaves as "doesn't exist"
    public async Task User_B_Cannot_Open_Or_Delete_User_As_Saved_Result()
    {
        var a = await _factory.CreateTestUserAsync();
        var b = await _factory.CreateTestUserAsync();
        var idOfA = await SeedSavedResponseAsync(a.Id);

        var client = _factory.CreateBrowserClient();
        await Web.LoginAsync(client, b.Email!, ReWasteFactory.TestUserPassword);

        // Opening it
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/MyDevices/Details/{idOfA}")).StatusCode);
        // The delete confirmation page
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/MyDevices/Delete/{idOfA}")).StatusCode);

        // Posting the delete directly (a token from any logged-in page with a form)
        var delete = await Web.PostFormAsync(client, "/Identity/Account/Manage/ChangePassword",
            "/MyDevices/DeleteConfirmed", new() { ["id"] = idOfA.ToString() });
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        // And the row is still there
        var stillThere = await _factory.WithDbAsync(db => db.SavedResponses.AnyAsync(s => s.Id == idOfA));
        Assert.True(stillThere);
    }

    [Fact] // IT-24: the positive side of IT-23
    public async Task Owner_Can_Open_And_Delete_Their_Saved_Result()
    {
        var a = await _factory.CreateTestUserAsync();
        var id = await SeedSavedResponseAsync(a.Id);

        var client = _factory.CreateBrowserClient();
        await Web.LoginAsync(client, a.Email!, ReWasteFactory.TestUserPassword);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/MyDevices/Details/{id}")).StatusCode);

        // The confirmation page carries the delete form (and its token)
        var delete = await Web.PostFormAsync(client, $"/MyDevices/Delete/{id}",
            "/MyDevices/DeleteConfirmed", new() { ["id"] = id.ToString() });

        Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);
        Assert.Equal("/MyDevices", Web.RedirectPath(delete));
        var exists = await _factory.WithDbAsync(db => db.SavedResponses.AnyAsync(s => s.Id == id));
        Assert.False(exists);
    }
}