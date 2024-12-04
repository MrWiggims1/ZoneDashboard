namespace ZoneLibrary.Services
{
    public class ZoneProductionService
    {
        private IProductionService _productionService = default!;
        
        public List<ProductionDepartment> Gen2ProductionDepartments =
        [
            new ProductionDepartment("Chassis",          [CardAreaOfOrigin.Chassis]),
            new ProductionDepartment("Cabinetry",        [CardAreaOfOrigin.CabsAssembly, CardAreaOfOrigin.SubAssembly, CardAreaOfOrigin.CabsPrep]),
            new ProductionDepartment("Bay 1",            [CardAreaOfOrigin.Bay1]),
            new ProductionDepartment("Electrical",       [CardAreaOfOrigin.Electrical]),
            new ProductionDepartment("Wall/Roof Mod",    [CardAreaOfOrigin.WallRoofMod]),
            new ProductionDepartment("Bay 3",            [CardAreaOfOrigin.Bay3, CardAreaOfOrigin.Toolbox]),
            new ProductionDepartment("Sealing",          [CardAreaOfOrigin.Sealing]),
            new ProductionDepartment("Upholstery",       [CardAreaOfOrigin.Upholstery]),
            new ProductionDepartment("Cabs Finishing",   [CardAreaOfOrigin.CabsFinishing]),
            new ProductionDepartment("Commissioning",    [CardAreaOfOrigin.Commissioning, CardAreaOfOrigin.Detailing]),
            new ProductionDepartment("Gas",              [CardAreaOfOrigin.Gas])
        ];

        public  List<ProductionDepartment> ExpoProductionDepartments =
        [
            new ProductionDepartment("Chassis",       [CardAreaOfOrigin.Chassis]),
            new ProductionDepartment("Cabinetry",     [CardAreaOfOrigin.CabsAssembly, CardAreaOfOrigin.SubAssembly, CardAreaOfOrigin.CabsPrep]),
            new ProductionDepartment("Wall/Roof Mod", [CardAreaOfOrigin.WallRoofMod]),
            new ProductionDepartment("Electrical",    [CardAreaOfOrigin.Electrical]),
            new ProductionDepartment("Bay 1",         [CardAreaOfOrigin.Bay1]),
            new ProductionDepartment("Bay 2",         [CardAreaOfOrigin.Bay2]),
            new ProductionDepartment("Bay 3/4",       [CardAreaOfOrigin.Bay3, CardAreaOfOrigin.Bay4, CardAreaOfOrigin.Upholstery]),
            new ProductionDepartment("Sealing",       [CardAreaOfOrigin.Sealing]),
            new ProductionDepartment("Cabs Finishing",[CardAreaOfOrigin.CabsFinishing]),
            new ProductionDepartment("Commissioning", [CardAreaOfOrigin.Commissioning, CardAreaOfOrigin.Detailing]),
            new ProductionDepartment("Gas",           [CardAreaOfOrigin.Gas])
        ];
        
        public ZoneProductionService(IProductionService productionService)
        {
            ArgumentNullException.ThrowIfNull(productionService);

            _productionService = productionService;
        }
        
        public async Task Initialize()
        {
            Log.Logger.Information("Initializing Zone Production service.");
            
            List<CardAreaOfOrigin> otherAreaGen2 = new List<CardAreaOfOrigin>();
            List<CardAreaOfOrigin> otherAreaExpo = new List<CardAreaOfOrigin>();

            foreach (CardAreaOfOrigin area in Enum.GetValues<CardAreaOfOrigin>())
            {
                if (!Gen2ProductionDepartments.Any(x => x.AreaOfOrigins.Contains(area)))
                    otherAreaGen2.Add(area);

                if (!ExpoProductionDepartments.Any(x => x.AreaOfOrigins.Contains(area)))
                    otherAreaExpo.Add(area);
            }

            if (otherAreaGen2.Count != 0)
                Gen2ProductionDepartments.Add(new ProductionDepartment("Other", otherAreaGen2));

            if (otherAreaExpo.Count != 0)
                ExpoProductionDepartments.Add(new ProductionDepartment("Other", otherAreaExpo));

            await _productionService.InitializeProductionService();
        }
    }
}