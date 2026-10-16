using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.Services;

namespace RE_Waste
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //telling the identity system to use our ApplicationUser class instead of the default IdentityUser class. This allows us to add custom properties to the user, such as Name, and have them stored in the database
            //roles has now been added to the identity system, allowing us to assign different roles to users 
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            // R3: postcode lookup through postcodes.io. Registered as a typed HTTP client so ASP.NET creates and reuses the HttpClient properly
            builder.Services.AddHttpClient<IPostcodeService, PostcodeService>(client =>
            {
                client.BaseAddress = new Uri("https://api.postcodes.io/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            var app = builder.Build();

            // Create the Manager role and the starter manager account (F4)
            // This is done here instead of in a controller so that it runs on app start-up, and not only when a manager visits the site.
            using (var scope = app.Services.CreateScope())
            {
                await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}
