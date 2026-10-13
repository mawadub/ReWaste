using RE_Waste.Models.Enums;
namespace RE_Waste.Models
{
    //this class is used to store information about a device that is being disposed of. It contains properties for the device's name, type, condition. The class also contains a constructor that initializes the properties with default values.
    //this class won't be used in the database, device won't become a table, but it will be used to store information about a device when wizard form will get filled.
    //if user click "Save" - SavedResponse table will store a copy of the phone details (brand, model, age, condition and so on) along with the routes and the date
    public class Device
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int AgeInYears { get; set; }
        public Condition Condition { get; set; }
        public bool HasPersonalData { get; set; }
        public bool HasBatteryDamage { get; set; }
    }
}
