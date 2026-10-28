using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Data
{
    // Runs on Startup: seeds the database with initial data for questions, recommendation rules, and drop-off points. Safe to run every time.
    public static class DataSeeder
    {
        // This method seeds the database with initial data for questions, recommendation rules, and drop-off points. It checks if each table is empty before adding data to avoid duplicates.
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Phone models come from a CSV so a data person can edit them without touching C#.
            if (!await context.PhoneModels.AnyAsync())
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "phone-models.csv");

                var models = File.ReadLines(path)
                    .Skip(1) // header row
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Split(','))
                    .Select(p => new PhoneModel
                    {
                        Brand = p[0].Trim(),
                        Name = p[1].Trim(),
                        ReleaseYear = int.TryParse(p[2], out var year) ? year : null
                    })
                    .DistinctBy(m => (m.Brand, m.Name))
                    .ToList();

                context.PhoneModels.AddRange(models);
                await context.SaveChangesAsync();
            }

            if (!await context.Questions.AnyAsync())
            {
                //1. Seed questions
                context.Questions.AddRange(
                    new Question
                    {
                        Text = "What is the phone brand?",
                        AnswerType = AnswerType.Text,
                        DeviceField = DeviceField.Brand,
                        DisplayOrder = 1
                    },
                    new Question
                    {
                        Text = "What is the phone model?",
                        AnswerType = AnswerType.Text,
                        DeviceField = DeviceField.Model,
                        DisplayOrder = 2
                    },
                    new Question
                    {
                        Text = "How old is the phone (in years)?",
                        AnswerType = AnswerType.Number,
                        DeviceField = DeviceField.AgeInYears,
                        DisplayOrder = 3
                    },
                    // 1.2 Seed the condition question with options
                    new Question
                    {
                        Text = "What is the phone's condition?",
                        AnswerType = AnswerType.Choice,
                        DeviceField = DeviceField.Condition,
                        DisplayOrder = 4,
                        Options = new List<AnswerOption>
                        {
                            CreateOption("Working", nameof(Condition.Working), 1),
                            CreateOption("Partly working", nameof(Condition.PartlyWorking), 2),
                            CreateOption("Not working", nameof(Condition.NotWorking), 3),
                            CreateOption("Damaged", nameof(Condition.Damaged), 4)
                        }
                    },
                    new Question
                    {
                        Text = "Does the phone contain personal data?",
                        AnswerType = AnswerType.Choice,
                        DeviceField = DeviceField.HasPersonalData,
                        DisplayOrder = 5,
                        Options = new List<AnswerOption>
                        {
                            CreateOption("Yes", bool.TrueString, 1),
                            CreateOption("No", bool.FalseString, 2)
                        }
                    },
                    new Question
                    {
                        Text = "Is the phone's battery damaged?",
                        AnswerType = AnswerType.Choice,
                        DeviceField = DeviceField.HasBatteryDamage,
                        DisplayOrder = 6,
                        Options = new List<AnswerOption>
                        {
                            CreateOption("Yes", bool.TrueString, 1),
                            CreateOption("No", bool.FalseString, 2)
                        }
                    });
            }

            // 2. Seed recommendation rules
            if (!await context.RecommendationRules.AnyAsync())
            {
                // Seed recommendation rules
                context.RecommendationRules.AddRange(
                    CreateRule("Damaged battery", RouteType.Recycle, "A damaged battery should be handled by a suitable recycling service.", 100, hasBatteryDamage: true),
                    CreateRule("Severely damaged phone", RouteType.Recycle, "The phone is damaged and should be assessed by a suitable recycling service.", 90, condition: Condition.Damaged),
                    CreateRule("Phone not working", RouteType.Repair, "A repair service may be able to restore the phone.", 80, condition: Condition.NotWorking),
                    CreateRule("Phone partly working", RouteType.Repair, "A repair service may be able to fix the phone.", 70, condition: Condition.PartlyWorking),
                    CreateRule("Phone is eight or more years old", RouteType.Recycle, "An older phone may be more suitable for responsible recycling.", 60, minAge: 8),
                    CreateRule("Phone is four to seven years old", RouteType.ResaleOrTradeIn, "Consider repair to extend the phone's useful life.", 50, minAge: 4, maxAge: 7, condition: Condition.Working),
                    CreateRule("Recent working phone", RouteType.Donate, "A working phone in good condition may be suitable for donation.", 40, condition: Condition.Working, maxAge: 3),
                    CreateRule("Working phone with personal data", RouteType.ResaleOrTradeIn, "Erase personal data before arranging a resale or trade-in.", 30, condition: Condition.Working, hasPersonalData: true),
                    CreateRule("Working phone without personal data", RouteType.ResaleOrTradeIn, "A working phone may be suitable for resale or trade-in.", 20, condition: Condition.Working, hasPersonalData: false),
                    CreateRule("General recycling fallback", RouteType.Recycle, "If another option is unsuitable, use a responsible recycling service.", 1));
            }
            // 3. Drop-off points. Real Scottish places (the pilot area is Scotland, per the requirement "UK (Scotland only) postcodes").
            // Names, addresses and postcodes come from each organisation's or council's own website (checked 17 Oct 2026).
            // Latitude/longitude are stored so distances can be worked out without calling the postcode service on every search.
            // If you add a point, look its postcode up at https://postcodes.io/postcodes/<POSTCODE> and copy the latitude/longitude.
            if (!await context.DropOffPoints.AnyAsync())
            {
                context.DropOffPoints.AddRange(
                    // Council recycling centres (small electricals accepted)
                    CreateDropOffPoint("Seafield Recycling Centre", "76 Seafield Road, Edinburgh", "EH6 7LW", 55.9748, -3.1500, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Craigmillar Recycling Centre", "Old Dalkeith Road, Edinburgh", "EH16 4TB", 55.9332, -3.1445, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Sighthill Recycling Centre", "Bankhead Avenue, Edinburgh", "EH11 4EA", 55.9239, -3.2967, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Bridge of Don Recycling Centre", "Scotstown Road, Bridge of Don, Aberdeen", "AB22 8NJ", 57.1830, -2.0990, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Hazlehead Recycling Centre", "Hazlehead Avenue, Aberdeen", "AB15 8BJ", 57.1370, -2.1830, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Bucksburn Recycling Centre", "Bankhead Avenue, Bucksburn, Aberdeen", "AB21 9ET", 57.1822, -2.1760, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Tullos Recycling Centre", "Greenbank Crescent, Tullos, Aberdeen", "AB12 3BG", 57.1111, -2.0877, DropOffPointType.RecyclingCentre),
                    // Repair
                    CreateDropOffPoint("iSmash Glasgow Buchanan Street", "Unit B, 249 Buchanan Street, Glasgow", "G1 2NG", 55.8627, -4.2515, DropOffPointType.Repair, "Mon-Sat 09:00-18:00, Sun 10:00-17:00", "https://www.ismash.com/pages/ismash-glasgow-buchanan-street"),
                    CreateDropOffPoint("iSmash Edinburgh St James Quarter", "St James Quarter, Edinburgh", "EH1 3SR", 55.9558, -3.1884, DropOffPointType.Repair, "Mon-Sat 09:00-18:00, Sun 10:00-18:00", "https://www.ismash.com/pages/ismash-edinburgh"),
                    // Charity
                    CreateDropOffPoint("British Heart Foundation Nicolson Street", "102 Nicolson Street, Edinburgh", "EH8 9EJ", 55.9455, -3.1830, DropOffPointType.Charity),
                    // Retailers (phone shops)
                    CreateDropOffPoint("Sky Silverburn", "Silverburn Shopping Centre, Barrhead Road, Glasgow", "G53 6EQ", 55.8265, -4.3410, DropOffPointType.Retailer, "Mon-Fri 10:00-20:00, Sat-Sun 10:00-18:00"),
                    CreateDropOffPoint("O2 Shop Silverburn", "Unit 19, Silverburn, 763 Barrhead Road, Glasgow", "G53 6AG", 55.8268, -4.3405, DropOffPointType.Retailer, "Mon-Sun 10:00-18:00 (Sat from 09:30)"));
            }

            // Save changes to the database
            await context.SaveChangesAsync();
        }

        // Helper methods to create AnswerOption, RecommendationRule, and DropOffPoint instances

        // Can be moved later to a separate class if needed for better organization and testability.
        private static AnswerOption CreateOption(string text, string value, int displayOrder)
        {
            return new AnswerOption
            {
                Text = text,
                Value = value,
                DisplayOrder = displayOrder
            };
        }

        private static RecommendationRule CreateRule(
            string name,
            RouteType route,
            string reason,
            int priority,
            Condition? condition = null,
            int? minAge = null,
            int? maxAge = null,
            bool? hasPersonalData = null,
            bool? hasBatteryDamage = null)
        {
            return new RecommendationRule
            {
                Name = name,
                Route = route,
                Reason = reason,
                Priority = priority,
                Condition = condition,
                MinAgeInYears = minAge,
                MaxAgeInYears = maxAge,
                HasPersonalData = hasPersonalData,
                HasBatteryDamage = hasBatteryDamage
            };
        }
        //updated constructor which takes opening hours and website
        private static DropOffPoint CreateDropOffPoint(
            string name,
            string address,
            string postcode,
            double latitude,
            double longitude,
            DropOffPointType pointType,
            string? openingHours = null,
            string? website = null)
        {
            return new DropOffPoint
            {
                Name = name,
                Address = address,
                Postcode = postcode,
                Latitude = latitude,
                Longitude = longitude,
                PointType = pointType,
                OpeningHours = openingHours,
                Website = website,
                IsActive = true
            };
        }
    }
}