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
                    CreateRule("Phone is four to seven years old", RouteType.Repair, "Consider repair to extend the phone's useful life.", 50, minAge: 4, maxAge: 7),
                    CreateRule("Recent working phone", RouteType.Donate, "A working phone in good condition may be suitable for donation.", 40, condition: Condition.Working, maxAge: 3),
                    CreateRule("Working phone with personal data", RouteType.ResaleOrTradeIn, "Erase personal data before arranging a resale or trade-in.", 30, condition: Condition.Working, hasPersonalData: true),
                    CreateRule("Working phone without personal data", RouteType.ResaleOrTradeIn, "A working phone may be suitable for resale or trade-in.", 20, condition: Condition.Working, hasPersonalData: false),
                    CreateRule("General recycling fallback", RouteType.Recycle, "If another option is unsuitable, use a responsible recycling service.", 1));
            }
            // 3. Seed drop-off points
            if (!await context.DropOffPoints.AnyAsync())
            {
                context.DropOffPoints.AddRange(
                    CreateDropOffPoint("Demo Charity Drop-off - London", "1 Sample Road, London", "SW1A 1AA", 51.5014, -0.1419, DropOffPointType.Charity),
                    CreateDropOffPoint("Demo Recycling Centre - Birmingham", "2 Sample Road, Birmingham", "B1 1AA", 52.4862, -1.8904, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Demo Retailer - Manchester", "3 Sample Road, Manchester", "M1 1AA", 53.4808, -2.2426, DropOffPointType.Retailer),
                    CreateDropOffPoint("Demo Repair Service - Leeds", "4 Sample Road, Leeds", "LS1 1AA", 53.8008, -1.5491, DropOffPointType.Repair),
                    CreateDropOffPoint("Demo Charity Drop-off - Glasgow", "5 Sample Road, Glasgow", "G1 1AA", 55.8642, -4.2518, DropOffPointType.Charity),
                    CreateDropOffPoint("Demo Recycling Centre - Liverpool", "6 Sample Road, Liverpool", "L1 1AA", 53.4084, -2.9916, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Demo Retailer - Bristol", "7 Sample Road, Bristol", "BS1 1AA", 51.4545, -2.5879, DropOffPointType.Retailer),
                    CreateDropOffPoint("Demo Repair Service - Sheffield", "8 Sample Road, Sheffield", "S1 1AA", 53.3811, -1.4701, DropOffPointType.Repair),
                    CreateDropOffPoint("Demo Charity Drop-off - Edinburgh", "9 Sample Road, Edinburgh", "EH1 1AA", 55.9533, -3.1883, DropOffPointType.Charity),
                    CreateDropOffPoint("Demo Recycling Centre - Cardiff", "10 Sample Road, Cardiff", "CF10 1AA", 51.4816, -3.1791, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Demo Retailer - Nottingham", "11 Sample Road, Nottingham", "NG1 1AA", 52.9548, -1.1581, DropOffPointType.Retailer),
                    CreateDropOffPoint("Demo Repair Service - Newcastle", "12 Sample Road, Newcastle", "NE1 1AA", 54.9783, -1.6178, DropOffPointType.Repair),
                    CreateDropOffPoint("Demo Charity Drop-off - Oxford", "13 Sample Road, Oxford", "OX1 1AA", 51.7520, -1.2577, DropOffPointType.Charity),
                    CreateDropOffPoint("Demo Recycling Centre - Cambridge", "14 Sample Road, Cambridge", "CB1 1AA", 52.2053, 0.1218, DropOffPointType.RecyclingCentre),
                    CreateDropOffPoint("Demo Retailer - Belfast", "15 Sample Road, Belfast", "BT1 1AA", 54.5973, -5.9301, DropOffPointType.Retailer));
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

        private static DropOffPoint CreateDropOffPoint(
            string name,
            string address,
            string postcode,
            double latitude,
            double longitude,
            DropOffPointType pointType)
        {
            return new DropOffPoint
            {
                Name = name,
                Address = address,
                Postcode = postcode,
                Latitude = latitude,
                Longitude = longitude,
                PointType = pointType,
                IsActive = true
            };
        }
    }
}