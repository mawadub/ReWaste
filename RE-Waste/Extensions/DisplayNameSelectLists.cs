using Microsoft.AspNetCore.Mvc.Rendering;
using RE_Waste.Models.Enums;

namespace RE_Waste.Extensions
{
    // Drop-down items that show enum values in plain words (e.g. "Partly working"
    // instead of "PartlyWorking"), using the ToDisplayName() extensions.
    // Each value is the enum's number, the same as Html.GetEnumSelectList, so model
    // binding and the pre-selected item on Edit pages keep working.
    public static class DisplayNameSelectLists
    {
        public static IEnumerable<SelectListItem> Routes() =>
            Enum.GetValues<RouteType>()
                .Select(route => new SelectListItem(route.ToDisplayName(), ((int)route).ToString()));

        public static IEnumerable<SelectListItem> Conditions() =>
            Enum.GetValues<Condition>()
                .Select(condition => new SelectListItem(condition.ToDisplayName(), ((int)condition).ToString()));

        public static IEnumerable<SelectListItem> PointTypes() =>
            Enum.GetValues<DropOffPointType>()
                .Select(type => new SelectListItem(type.ToDisplayName(), ((int)type).ToString()));
    }
}
