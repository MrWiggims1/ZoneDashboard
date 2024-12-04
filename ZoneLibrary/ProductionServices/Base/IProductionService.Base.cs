using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace ZoneProductionLibrary.ProductionServices.Base
{
    public partial interface IProductionService
    {
        /// <summary>
        /// Key is the Van name
        /// </summary>
        public Dictionary<string, VanProductionInfo> ProductionVans { get; }
        public ConcurrentDictionary<string, TrelloMember> Members { get; }
        public int VanCount { get; }
        public int RedCardCount { get; }
        public int JobCardCount { get; }
        public int CheckCount { get; }

        public VanProductionInfo GetVanInfo(VanBoard van) => ProductionVans[van.Name];

        Task InitializeProductionService();

        List<ProductionDepartment> Gen2ProductionDepartments { get; set; }
        List<ProductionDepartment> ExpoProductionDepartments { get; set; }

        public static readonly string FileBasePath = "./wwwroot/";

        public Task DownloadTrelloFileAsync(string url, string path);
    }
}