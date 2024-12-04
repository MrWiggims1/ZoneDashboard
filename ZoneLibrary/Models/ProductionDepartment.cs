namespace ZoneProductionLibrary.Models
{
    public class ProductionDepartment(string name, List<CardAreaOfOrigin> areaOfOrigins)
    {
        public string Name { get; set; } = name;
        public List<CardAreaOfOrigin> AreaOfOrigins { get; set; } = areaOfOrigins;
    }
}
