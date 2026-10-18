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


        //a. Battery first. If device.HasBatteryDamage is true, return a list with only Recycle and a safety reason.
        //The requirement says a damaged battery means a "recycling-only route", so don't add alternatives.

        public Task<IReadOnlyList<RouteRecommendation>> RecommendAsync(Device device)
        {
            //list of recomendations
            List<RouteRecommendation> list = new List<RouteRecommendation>();

            //throw new NotImplementedException();
            if ( device.HasBatteryDamage ) //condition for if the device has a broken battery
            {

                //route recommendation for recyling
                RouteRecommendation recycleRec = new RouteRecommendation() { Route = Enums.RouteType.Recycle, 
                                                                             Reason = "Since your device contains a damaged battery we suggest you recycle your device at the nearest recylcing point." }; 
                list.Add(recycleRec); // add to list

                
                return Task.FromResult<IReadOnlyList<RouteRecommendation>>(list); // return list

            }

            List<RecommendationRule> ActiveRules = _context.RecommendationRules.ToList(); //loads all of the Active Rules

            List<RecommendationRule> ValidRules = new List<RecommendationRule>(); //empty list to be filled with any valid rules
            
            foreach (RecommendationRule rule in ActiveRules)
            {
                if(rule.Matches(device)) //check if user device meets the conditions of this rule
                {
                    ValidRules.Add(rule); //since rule applies to this device we add it to valid rule list
                }
            }

            if (ValidRules.Count == 0) //if no rules matched and validrules is empty we go down this route
            {
                RouteRecommendation recycleRec = new RouteRecommendation() //create a generic route that says recycle and gives generic reason
                {
                    Route = Enums.RouteType.Recycle,
                    Reason = "Since our system does not have much information on your device we suggest you recycle your device at the nearest recylcing point."
                };

                list.Add(recycleRec);

                return Task.FromResult<IReadOnlyList<RouteRecommendation>>(list); //return the list with this generic route recomendation
            }

            //get top priority recomendations:

            var recommendations = ValidRules.OrderByDescending(rule => rule.Priority) //orders the rules by highest priority
                                            .GroupBy(rule => rule.Route) // groups  the rules by their assosiated route
                                            .Select(group => group.First()) // groups each type of route with the highest priority version
                                            .Take(3)// takes at most 3
                                            .Select(rule => new RouteRecommendation
                                                    {
                                                        Route = rule.Route,
                                                        Reason = rule.Reason
                                                    }) //turns them into route recomendations
                                            .ToList(); //turns them into lists

            //now we have recomendations we can put them in the list then return the list.

            return Task.FromResult<IReadOnlyList<RouteRecommendation>>(recommendations); //return list
        }
    }
}