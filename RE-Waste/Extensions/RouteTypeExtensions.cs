using RE_Waste.Models.Enums;   
namespace RE_Waste.Extensions
{
    //Turns a RouteType into the words a user should see 
    public static class RouteTypeExtensions
    {
        public static string ToDisplayName(this RouteType route) => route switch
        {
            RouteType.Donate => "Donate",
            RouteType.ResaleOrTradeIn => "Resale or trade-in",
            RouteType.Repair => "Repair",
            RouteType.Recycle => "Recycle",
            _ => route.ToString()
        };

    }
}
