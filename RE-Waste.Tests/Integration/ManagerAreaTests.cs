using System.Net;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Tests.Integration;

public class ManagerAreaTests : IClassFixture<ReWasteFactory>
{
    private readonly ReWasteFactory _factory;
    public ManagerAreaTests(ReWasteFactory factory) => _factory = factory;

    private async Task<HttpClient> ManagerClientAsync()
    {
        var client = _factory.CreateBrowserClient();
        await Web.LoginAsync(client, ReWasteFactory.TestManagerEmail, ReWasteFactory.TestManagerPassword);
        return client;
    }

    [Fact] // IT-30
    public async Task Manager_Can_Create_A_Rule()
    {
        var client = await ManagerClientAsync();
        var name = "Test rule " + Guid.NewGuid().ToString("N");

        var response = await Web.PostFormAsync(client, "/Manager/Rules/Create", "/Manager/Rules/Create",
            new()
            {
                ["Name"] = name,
                ["Route"] = "Recycle",
                ["Reason"] = "Created by an integration test",
                ["Priority"] = "55"
            });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Manager/Rules", Web.RedirectPath(response));

        var rule = await _factory.WithDbAsync(db => db.RecommendationRules.SingleAsync(r => r.Name == name));
        Assert.Equal(RouteType.Recycle, rule.Route);
        Assert.Equal(55, rule.Priority);
        Assert.True(rule.IsActive); // new rules start active
    }

    [Fact] // IT-31
    public async Task Invalid_Rule_Is_Not_Saved_And_The_Form_Is_Shown_Again()
    {
        var client = await ManagerClientAsync();
        var before = await _factory.WithDbAsync(db => db.RecommendationRules.CountAsync());

        var response = await Web.PostFormAsync(client, "/Manager/Rules/Create", "/Manager/Rules/Create",
            new() { ["Name"] = "", ["Route"] = "Recycle", ["Reason"] = "", ["Priority"] = "5" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // same page, with validation messages
        var after = await _factory.WithDbAsync(db => db.RecommendationRules.CountAsync());
        Assert.Equal(before, after);
    }

    [Fact] // IT-32
    public async Task Manager_Can_Switch_A_Rule_Off_And_On()
    {
        var client = await ManagerClientAsync();
        var id = await _factory.WithDbAsync(async db =>
        {
            var rule = new RecommendationRule
            {
                Name = "Toggle me " + Guid.NewGuid().ToString("N"),
                Route = RouteType.Recycle,
                Reason = "test",
                Priority = 1,
                IsActive = true
            };
            db.RecommendationRules.Add(rule);
            await db.SaveChangesAsync();
            return rule.Id;
        });

        // The Rules list page contains the toggle forms (and so a token)
        var response = await Web.PostFormAsync(client, "/Manager/Rules", "/Manager/Rules/ToggleActive",
            new() { ["id"] = id.ToString() });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var isActive = await _factory.WithDbAsync(async db => (await db.RecommendationRules.FindAsync(id))!.IsActive);
        Assert.False(isActive);
    }

    [Fact] // IT-33: dashboard shows counts only, never saved results
    public async Task Dashboard_Does_Not_Expose_Saved_Result_Details()
    {
        var user = await _factory.CreateTestUserAsync();
        await _factory.WithDbAsync(async db =>
        {
            db.SavedResponses.Add(new SavedResponse
            {
                Brand = "SecretBrandXyz",
                Model = "m",
                AgeInYears = 1,
                Condition = Condition.Working,
                PostcodeDistrict = "EH1",
                Route1 = RouteType.Recycle,
                Reason1 = "SecretReasonXyz",
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            });
            await db.SaveChangesAsync();
        });
        var client = await ManagerClientAsync();

        var response = await client.GetAsync("/Manager");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await Web.TextAsync(response);
        Assert.DoesNotContain("SecretBrandXyz", text);
        Assert.DoesNotContain("SecretReasonXyz", text);
        Assert.DoesNotContain(user.Email!, text);
    }
}