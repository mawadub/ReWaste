using System.Net;
using System.Text.RegularExpressions;

namespace RE_Waste.Tests.Integration;

public static class Web
{
    // Finds <input ... name="__RequestVerificationToken" ... value="XYZ" /> in a page.
    private static readonly Regex TokenPattern = new(
        "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    // The page text with HTML entities decoded, so "couldn&#x27;t" compares as "couldn't".
    public static async Task<string> TextAsync(HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    // Where a 302 points, as a path ("/Identity/Account/Login"), ignoring ?ReturnUrl=...
    public static string RedirectPath(HttpResponseMessage response)
    {
        Assert.NotNull(response.Headers.Location);
        return new Uri(new Uri("https://localhost"), response.Headers.Location!).AbsolutePath;
    }

    // GET a page that contains a form and read its hidden anti-forgery token.
    // (The matching anti-forgery COOKIE is stored by the client automatically.)
    public static async Task<string> GetTokenAsync(HttpClient client, string pageWithForm)
    {
        var response = await client.GetAsync(pageWithForm);
        var html = await response.Content.ReadAsStringAsync();
        var match = TokenPattern.Match(html);

        Assert.True(match.Success,
            $"No anti-forgery token on {pageWithForm} (status {(int)response.StatusCode}). Is the form missing or the page redirecting?");

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    // POST a form the way a browser does: fetch the page for its token, then send fields + token.
    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string pageWithForm, string postUrl, Dictionary<string, string> fields)
    {
        var token = await GetTokenAsync(client, pageWithForm);
        var body = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = token };
        return await client.PostAsync(postUrl, new FormUrlEncodedContent(body));
    }

    // Log in for real through the Identity login form. Success is a 302 redirect.
    public static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var response = await PostFormAsync(client, "/Identity/Account/Login", "/Identity/Account/Login",
            new() { ["Input.Email"] = email, ["Input.Password"] = password });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    // ---- Wizard helpers ----

    // POST one answer to the wizard's current step.
    public static Task<HttpResponseMessage> AnswerAsync(HttpClient client, string answer) =>
        PostFormAsync(client, "/Wizard/Step", "/Wizard/Next", new() { ["answer"] = answer });

    // Walk the whole wizard: start, six answers, postcode, then "Recommend".
    // Defaults describe a recent working phone with no personal data and no battery damage.
    // Leaves the client sitting on /Wizard/Results (session holds the result).
    public static async Task CompleteWizardAsync(
        HttpClient client,
        string brand = "Apple", string model = "iPhone 6s", string age = "2",
        string condition = "Working", string personalData = "False", string batteryDamage = "False",
        string postcode = "EH1 2NG")
    {
        await client.GetAsync("/Wizard/Start");   // resets/creates the wizard state in the session

        foreach (var answer in new[] { brand, model, age, condition, personalData, batteryDamage })
        {
            var next = await AnswerAsync(client, answer);
            Assert.Equal(HttpStatusCode.Redirect, next.StatusCode);
        }

        var postcodeResponse = await PostFormAsync(client, "/Wizard/Postcode", "/Wizard/Postcode",
            new() { ["postcode"] = postcode });
        Assert.Equal(HttpStatusCode.Redirect, postcodeResponse.StatusCode);

        var recommend = await PostFormAsync(client, "/Wizard/Complete", "/Wizard/Recommend", new());
        Assert.Equal(HttpStatusCode.Redirect, recommend.StatusCode);
        Assert.Equal("/Wizard/Results", RedirectPath(recommend));
    }
}