using Microsoft.IdentityModel.Logging;
using RE_Waste.Data;
using RE_Waste.Services;
using System.Collections.Generic;

namespace RE_Waste.Models
{

    public class RecommendationService : IRecommendationService
    {

        public RecommendationService(ApplicationDbContext dbcontext) 
        {
            ApplicationDbContext wdb = dbcontext;
        }
        //a. Battery first. If device.HasBatteryDamage is true, return a list with only Recycle and a safety reason.
        //The requirement says a damaged battery means a "recycling-only route", so don't add alternatives.

        public Task<IReadOnlyList<RouteRecommendation>> RecommendAsync(Device device)
        {
            //list of recomendations
            IReadOnlyList<RouteRecommendation> list = new List<RouteRecommendation>();

            //throw new NotImplementedException();
            if ( device.HasBatteryDamage ) //condition for if the device has a broken battery
            {

                //route recommendation for recyling
                RouteRecommendation recycleRec = new RouteRecommendation() { Route = Enums.RouteType.Recycle, 
                                                                             Reason = "Since your device contains a damaged battery we suggest you recycle your device at the nearest recylcing point." }; 
                list.Append(recycleRec); // add to list

                
                return Task.FromResult(list); // return list

            }

            return Task.FromResult(list);
        }
    }
}