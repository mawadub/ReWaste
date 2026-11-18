using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.Services;


namespace RE_Waste.Tests.Integration
{
    // Starts the REAL Re-Waste app inside the test process, but with test-friendly replacements:
    //  - an in-memory database instead of SQL Server
    //  - a fake postcode lookup instead of calling postcodes.io
    public class ReWasteFactory : WebApplicationFactory<Program>
    {
        // Dedicated test accounts
        public const string TestManagerEmail = "manager@test.local";
        public const string TestManagerPassword = "Test123!";
        public const string TestUserPassword = "Test123!";

        // Each test run gets its own database, so that tests can run in parallel without interfering with each other
        private readonly string _db = "ReWasteTests-" + Guid.NewGuid(); 

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing"); // so that we don't accidentally use the real database or call postcodes.io

            //When IdentitySeeder is called, it will read the email and password from the configuration.
            //Here we are overriding the configuration to provide our test values.
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SeedManager:Email"] = TestManagerEmail,
                    ["SeedManager:Password"] = TestManagerPassword,
                });
            });

            builder.ConfigureServices(services =>
            {
                //1. Remove the real database context registration
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_db));

                //2. Remove the real postcode service registration
                services.RemoveAll<IPostcodeService>();
                //3. Add a fake postcode service that returns a fixed result for testing
                services.AddSingleton<IPostcodeService, FakePostcodeService>();
            });
        }

        //create a browser-like client that can be used to send HTTP requests to the test server.
        //This client will automatically follow redirects, handle cookies, and manage other aspects of HTTP communication
        public HttpClient CreateBrowserClient(bool followRedirect = false) =>
            CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = followRedirect,
                HandleCookies = true,
                BaseAddress = new Uri("https://localhost"),
            });

        //unique email address for a new test user, so that tests can run in parallel without interfering with each other
        public static string NewEmail() => $"user-{Guid.NewGuid():N}@test.local";

        //insert a normal user into the database
        public async Task<ApplicationUser> CreateTestUserAsync(string? email = null)
        {
            //if the email wasn't passed in, generage a new unique Email address for the test user
            email ??= NewEmail();
            //create a new scope to get the UserManager service, which is used to create a new user in the database
            using var scope = Services.CreateScope();
            //get the UserManager service from the scope
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //create a new ApplicationUser
            var user = new ApplicationUser { UserName = email, Email = email, Name = "Test User", EmailConfirmed = true};
            //create the user in the database with the test password
            var result = await userManager.CreateAsync(user, TestUserPassword);
            //if the user creation failed, throw an exception with the error messages
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
            //return the created user
            return user;
        }

        //create a connection to the test database, so test can put data in the database before making HTTP requests to the test server
        public async Task WithDbAsync(Func<ApplicationDbContext, Task> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await action(db);
        }
       
        public async Task<T> WithDbAsync<T>(Func<ApplicationDbContext, Task<T>> func)
        {
            using var scope = Services.CreateScope();
            return await func(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        }

    }
    // A fake postcode service that returns a fixed result for testing
    //will know Edinburgh is a valid postcode, but will return "not found" for any other postcode
    public class FakePostcodeService : IPostcodeService
    {
        public Task <GeoLocation?> LookupAsync (string postcode)
        {
            var cleaned = (postcode ?? string.Empty).Trim().ToUpperInvariant();
            GeoLocation? result = cleaned.StartsWith("EH") ? new GeoLocation { Latitude = 55.9533, Longitude = -3.1883 } : null;
            return Task.FromResult(result);
        }
    }
}
