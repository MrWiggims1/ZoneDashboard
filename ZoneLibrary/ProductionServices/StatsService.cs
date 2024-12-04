namespace ZoneProductionLibrary.ProductionServices
{
    public class StatsService
    {
        public CardFilterOptions CardFilterOptions { get; } = new CardFilterOptions();
        public BoardFilterOptions BoardFilterOptions { get; } = new BoardFilterOptions();
        
        public List<string> SelectedDepartments { get; set; } = [];
        public TypeOfVan SelectedVanType { get; set; } = TypeOfVan.Gen2;

        public bool DetailedButtonDropDown { get; set; } = true;
    }
}
