using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using RE_Waste.Services;

namespace RE_Waste.Tests.Integration
{
    public class WizardTests : IClassFixture<ReWasteFactory>
    {
        private readonly ReWasteFactory _factory;
        public WizardTests(ReWasteFactory factory)
        {
            _factory = factory;
        }

        [Fact] //IT-10: Check that wizard loads and start gets user to the first question page
        public async Task Start_Redirects_To_first_page_and_renders()
        {
            var client = _factory.CreateBrowserClient();
            var start = await client.GetAsync("/Wizard/Start");
            Assert.Equal("/Wizard/Step", Web.RedirectPath(start));

            var step = await client.GetAsync("/Wizard/Step");
            Assert.Equal(HttpStatusCode.OK, step.StatusCode);
            Assert.Contains("What is the phone brand?", await Web.TextAsync(step));
        }

        [Fact] //IT-11: Empty answer shows wrror and stays on the same question
        public async Task Empty_Answer_Check()
        {
            var client = _factory.CreateBrowserClient();
            await client.GetAsync("/Wizard/Start");
            var response = await Web.AnswerAsync(client, "");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(AnswerValidator.RequiredMessage, await Web.TextAsync(response));
        }

        [Fact] //IT-12: If you press next without a session it will get you to the start
        public async Task Next_Without_The_Session()
        {
            var client = _factory.CreateBrowserClient();
            //here we would call /wizard/start, but we never do, so no wizard state in the session
            //check on a few pages with forms, they should give a valid anti-forgery token to post with
            var response = await Web.PostFormAsync(client, "/Identity/Account/Login", "/Wizard/Next",
                new() { ["answer"] = "Apple" });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Wizard/Start", Web.RedirectPath(response));
        }

        [Fact] // IT-13
        public async Task Next_Without_A_Session_Sends_The_User_Back_To_Start()
        {
            var client = _factory.CreateBrowserClient();
            // Never called /Wizard/Start, so no wizard state in the session (like an expired session).
            // Any page with a form gives us a valid anti-forgery token to post with.
            var response = await Web.PostFormAsync(client, "/Identity/Account/Login", "/Wizard/Next",
                new() { ["answer"] = "Apple" });

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Wizard/Start", Web.RedirectPath(response));
        }

        [Fact] //IT-14: Check if system handles invalid postcodes correctly
        public async Task Invalid_Postcode_Show_Message()
        {
            var client = _factory.CreateBrowserClient();
            await client.GetAsync("/Wizard/Start");
            var response = await Web.PostFormAsync(client, "/Wizard/Postcode", "/Wizard/Postcode",
                new() { ["postcode"] = "12345" });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Please enter a valid postcode.", await Web.TextAsync(response));
        }

        [Fact] //IT-15: Full jorney for a recent working phone, make sure recomends donation first
        public async Task Full_Journey_For_A_Recent_Working_Phone_Recommends_Donate_First()
        {
            var client = _factory.CreateBrowserClient();

            await Web.CompleteWizardAsync(client); // Apple iPhone 6s, 2 yrs, Working, no personal data, no battery damage

            var results = await client.GetAsync("/Wizard/Results");

            Assert.Equal(HttpStatusCode.OK, results.StatusCode);
            var text = await Web.TextAsync(results);
            // The seeded rule "Recent working phone" (Donate, priority 40) outranks the resale rule (20).
            Assert.Contains("A working phone in good condition may be suitable for donation.", text);
            Assert.Contains("A working phone may be suitable for resale or trade-in.", text);
        }

        [Fact] // IT-16: a damaged battery must win over everything (priority 100)
        public async Task Damaged_Battery_Recommends_Recycling()
        {
            var client = _factory.CreateBrowserClient();

            await Web.CompleteWizardAsync(client, batteryDamage: "True");

            var results = await client.GetAsync("/Wizard/Results");
            var text = await Web.TextAsync(results);
            var title = System.Text.RegularExpressions.Regex.Match(text, "<h1[^>]*>(.*?)</h1>", System.Text.RegularExpressions.RegexOptions.Singleline).Groups[1].Value;
            Console.WriteLine("H1 = " + title);
            Console.WriteLine(text.Length > 4000 ? text.Substring(text.IndexOf("<main"), 2500) : text);
            Assert.True(text.Contains("recycling", StringComparison.OrdinalIgnoreCase),
            "Results page did not mention recycling. Page text: " +
            System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ").Replace("\r", " ").Replace("\n", " ").Substring(0, 1500));
        }

        [Fact] // IT-17: If user tries to go to the results without answering the questions it will be redirected to the start
        public async Task Results_Without_A_Finished_Wizard_Redirects_To_Start()
        {
            var client = _factory.CreateBrowserClient();

            var response = await client.GetAsync("/Wizard/Results");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Wizard/Start", Web.RedirectPath(response));
        }
    }
}
