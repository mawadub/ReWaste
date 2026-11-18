using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RE_Waste.Tests.Integration
{
    public class AccessControlTests : IClassFixture<ReWasteFactory>
    {
        private readonly ReWasteFactory _factory;
        public AccessControlTests(ReWasteFactory factory) => _factory = factory;

        //pages that only manager can access
        public static IEnumerable<object[]> ManagerOnlyPages => new []
        {
            new object[] { "/Manager" },
            new object[] { "/Manager/Rules" },
            new object[] { "/Manager/Questions" },
            new object[] { "/Manager/DropOffPoints" },
        };

        [Fact] //IT-00: to check the app starts and the home page renders
        public async Task Home_Page_Loads()
        {
            var client = _factory.CreateBrowserClient();
            var response = await client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
       
        [Fact] //IT-01: My devices won't open for visitor
        public async Task Anonymous_Visitor_Redirected_To_Login_From_MyDevices()
        {
            var client = _factory.CreateBrowserClient();
            var response = await client.GetAsync("/MyDevices");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Identity/Account/Login", Web.RedirectPath(response));
        }

        [Theory] //IT-02: Manager pages won't open for visitors
        [MemberData(nameof(ManagerOnlyPages))]
        public async Task Annonymous_Visitor_Redirected_to_Login_From_Manager(string url)
        {
            var client = _factory.CreateBrowserClient();
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Identity/Account/Login", Web.RedirectPath(response));
        }

        [Theory] //IT-03: Manager pages won't open for loged in users, who are not manager
        [MemberData(nameof(ManagerOnlyPages))]
        public async Task Normal_User_is_Refused_on_Managers_Pages(string url)
        {
            //create a user account in the db
            var user = await _factory.CreateTestUserAsync();
            //create a browser client session
            var client = _factory.CreateBrowserClient();
            //login the user in the created browser
            await Web.LoginAsync(client, user.Email!, ReWasteFactory.TestUserPassword);
            //get the responses as loged in user
            var response = await client.GetAsync(url);
            // Identity answers "authenticated but not allowed" with a redirect to the access-denied page.
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Identity/Account/AccessDenied", Web.RedirectPath(response));
        }

        [Theory] //IT-04: Manager pages opens for manager
        [MemberData(nameof(ManagerOnlyPages))]
        public async Task Manager_can_open_Managers_Pages(string url)
        {
            var client = _factory.CreateBrowserClient();
            await Web.LoginAsync(client, ReWasteFactory.TestManagerEmail, ReWasteFactory.TestManagerPassword);
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact] //IT-05: Refestered user can sign in and access device page
        public async Task Seeded_User_can_Sign_in()
        {
            var user = await _factory.CreateTestUserAsync();
            var client = _factory.CreateBrowserClient();
            await Web.LoginAsync(client, user.Email!, ReWasteFactory.TestUserPassword);
            var myDevices = await client.GetAsync("/MyDevices");
            Assert.Equal(HttpStatusCode.OK, myDevices.StatusCode);
        }

        [Fact] //IT-06: Wrong password is rejected and user is not logged in
        public async Task Wrong_Password_Rejections()
        {
            var user = await _factory.CreateTestUserAsync();
            var client = _factory.CreateBrowserClient();
            var response = await Web.PostFormAsync(client, "/Identity/Account/Login", "/Identity/Account/Login",
            new() { ["Input.Email"] = user.Email!, ["Input.Password"] = "Wrong-password-1!" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // the form is shown again, no redirect

            var myDevices = await client.GetAsync("/MyDevices");
            Assert.Equal(HttpStatusCode.Redirect, myDevices.StatusCode); // still not logged in
        }

        [Fact] //IT-07: Regestering creates a new account
        public async Task Regestering_Creates_The_Account()
        {
            var email = ReWasteFactory.NewEmail();
            var client = _factory.CreateBrowserClient();
            var response = await Web.PostFormAsync(client, "/Identity/Account/Register", "/Identity/Account/Register",
                new()
                {
                    ["Input.Name"] = "New Person",
                    ["Input.Email"] = email,
                    ["Input.Password"] = ReWasteFactory.TestUserPassword,
                    ["Input.ConfirmPassword"] = ReWasteFactory.TestUserPassword
                });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); 
            var exists = await _factory.WithDbAsync(db => db.Users.AnyAsync(u => u.Email == email));
            Assert.True(exists);
        }
    }
}
