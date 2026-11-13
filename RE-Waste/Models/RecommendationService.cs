using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using RE_Waste.Data;
using RE_Waste.Services;
using System.Collections.Generic;
using System.Linq;

namespace RE_Waste.Models
{

    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context; //empty local read only copy of db

        public RecommendationService(ApplicationDbContext dbcontext)  //constructor
        {
            _context = dbcontext; // here we fill the read only local copy of db with the db
        }

        //the below method exists to test this RecommendAsync
        public void test()
        {
            Device device = new Device() { Brand = "Google", Model = "Google", AgeInYears = 1, Condition = Enums.Condition.Working, HasBatteryDamage = false, HasPersonalData = true };

            Task<IReadOnlyList<RouteRecommendation>> recs = RecommendAsync(device);

            Console.WriteLine("Here is the output");
            Console.WriteLine(recs);

        }

        public async Task<IReadOnlyList<RouteRecommendation>> RecommendAsync(Device device)
        {
            //list of recomendations
            List<RouteRecommendation> list = new List<RouteRecommendation>();

            // ----------------------------------------------------------------
            // ABSOLUTE RULE CHECK
            // Damaged battery always recommends Recycle. This stays hardcoded
            // as a safety-critical override, but absolute rules from the DB
            // will also be prioritized below.
            // ----------------------------------------------------------------
            if (device.HasBatteryDamage)
            {
                RouteRecommendation recycleRec = new RouteRecommendation()
                {
                    Route = Enums.RouteType.Recycle,
                    Reason = "Since your device contains a damaged battery we suggest you recycle your device at the nearest recylcing point."
                };
                list.Add(recycleRec);
                return list;
            }

            // Load all active rules from the database.
            List<RecommendationRule> activeRules = await _context.RecommendationRules
                .Where(r => r.IsActive)
                .ToListAsync();

            // Score every rule. Key = rule, Value = MatchScore (null means "contradicted").
            var scoredRules = activeRules
                .Select(rule => new { Rule = rule, Score = rule.MatchScore(device) })
                .Where(x => x.Score.HasValue)  // drop contradicted rules
                .ToList();

            // ----------------------------------------------------------------
            // ABSOLUTE RULES from DB: if any IsAbsolute rule matched (score >= 0),
            // return only those, ordered by specificity (score), then priority.
            // ----------------------------------------------------------------
            var absoluteMatches = scoredRules
                .Where(x => x.Rule.IsAbsolute)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Rule.Priority)
                .ToList();

            if (absoluteMatches.Count > 0)
            {
                return absoluteMatches
                    .GroupBy(x => x.Rule.Route)
                    .Select(g => g.First())
                    .Take(3)
                    .Select(x => new RouteRecommendation
                    {
                        Route = x.Rule.Route,
                        Reason = x.Rule.Reason
                    })
                    .ToList();
            }

            // ----------------------------------------------------------------
            // NORMAL RANKING: order by score (closest match first),
            // then by Priority as tie-breaker for equally-matched rules.
            // ----------------------------------------------------------------
            if (scoredRules.Count == 0)
            {
                // No rules matched at all — fallback.
                RouteRecommendation recycleRec = new RouteRecommendation()
                {
                    Route = Enums.RouteType.Recycle,
                    Reason = "Since our system does not have much information on your device we suggest you recycle your device at the nearest recylcing point."
                };
                list.Add(recycleRec);
                return list;
            }

            var recommendations = scoredRules
                .OrderByDescending(x => x.Score)            // closest match first
                .ThenByDescending(x => x.Rule.Priority)     // tie-break by priority
                .GroupBy(x => x.Rule.Route)                  // one per route
                .Select(g => g.First())                      // best rule per route
                .Take(3)                                     // at most 3 routes
                .Select(x => new RouteRecommendation
                {
                    Route = x.Rule.Route,
                    Reason = x.Rule.Reason
                })
                .ToList();

            return recommendations;

        }
    }
}