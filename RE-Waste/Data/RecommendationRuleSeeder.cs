
using Microsoft.EntityFrameworkCore;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Data
{
    //this seeder provides sample and ONLY test data for testing Recommend Async. This data should be changed for final appliaction.
    public static class RecommendationRuleSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // Do not add the test rules again if rules already exist.
            if (await context.RecommendationRules.AnyAsync())
            {
                return;
            }

            var rules = new List<RecommendationRule>
            {
                // 1. ReSell: older devices that are not battery-damaged.
                new RecommendationRule
                {
                    Name = "Test - Resell Older Device",
                    Route = RouteType.ResaleOrTradeIn,
                    Reason = "This device may still have resale value.",
                    Priority = 50,
                    MinAgeInYears = 3,
                    MaxAgeInYears = 8,
                    HasBatteryDamage = false,
                    IsActive = true
                },

                // 2. Recycle: older devices that are not battery-damaged.
                new RecommendationRule
                {
                    Name = "Test - Recycle Older Device",
                    Route = RouteType.Recycle,
                    Reason = "This older device may be suitable for recycling.",
                    Priority = 45,
                    MaxAgeInYears = 9,
                    MinAgeInYears = 9,
                    HasBatteryDamage = false,
                    IsActive = false
                },

                // 3. Donate: devices without personal data.
                new RecommendationRule
                {
                    Name = "Test - Donate Device",
                    Route = RouteType.Donate,
                    Reason = "This device may be suitable for donation.",
                    Priority = 40,
                    HasPersonalData = false,
                    HasBatteryDamage = false,
                    IsActive = true
                },

                // 4. Repair: devices up to five years old.
                new RecommendationRule
                {
                    Name = "Test - Repair Device",
                    Route = RouteType.Repair,
                    Reason = "Repair may help extend this device's life.",
                    Priority = 30,
                    MaxAgeInYears = 5,
                    HasBatteryDamage = false,
                    IsActive = true
                },

                // 5. General recycling: matches any device.
                // This deliberately overlaps with the other recycle rule.
                new RecommendationRule
                {
                    Name = "Test - General Recycling",
                    Route = RouteType.Recycle,
                    Reason = "Recycling is an option for this device.",
                    Priority = 10,
                    IsActive = false
                }
            };

            context.RecommendationRules.AddRange(rules);

            await context.SaveChangesAsync();
        }
    }
}
