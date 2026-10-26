namespace RE_Waste.Models
{
    // Stores the fixed list of phone brands used by the wizard.
    public class PhoneBrands
    {
        // The last option, for a brand that isn't in the list.
        public const string Other = "Other";

        public static readonly string[] All =
{
            "Apple",
            "Samsung",
            "Google",
            "Huawei",
            "Xiaomi",
            "OnePlus",
            "Oppo",
            "Motorola",
            "Nokia",
            "Sony",
            Other
        };

        // True for a brand from the list; false for "Other", a typed-in brand or nothing.
        public static bool IsListed(string? brand) =>
            brand != null && brand != Other && All.Contains(brand);
    }
}
