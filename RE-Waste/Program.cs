using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using static System.Formats.Asn1.AsnWriter;

namespace RE_Waste
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

            // Why are we using dynamic typing for the builder
            // WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //telling the identity system to use our ApplicationUser class instead of the default IdentityUser class. This allows us to add custom properties to the user, such as Name, and have them stored in the database
            //roles has now been added to the identity system, allowing us to assign different roles to users 
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();


            // Why are we using dynamic typing for the app
            // WebApplication app = builder.Build();
            var app = builder.Build();

            // Create the Manager role and the starter manager account (F4)
            // Create the initial data in the database if it doesn't exist yet (F5)
            // This is done here instead of in a controller so that it runs on app start-up, and not only when a manager visits the site.
            using (var scope = app.Services.CreateScope())
            {
                //creates or updates the database from the migrations and does nothing if it’s already up to date
                await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
                await IdentitySeeder.SeedAsync(scope.ServiceProvider);
                await DataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
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
