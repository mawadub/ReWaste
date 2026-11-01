namespace RE_Waste.ViewModels
{
    // G5: the numbers on the Manager dashboard.
    // Saved results is only a total: managers never see the saved results or who owns them.
    public class ManagerDashboardViewModel
    {
        public int ActiveQuestions { get; set; }
        public int ActiveRules { get; set; }
        public int DropOffPoints { get; set; }
        public int SavedResults { get; set; }
    }
}
