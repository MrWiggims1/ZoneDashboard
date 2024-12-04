namespace ZoneProductionLibrary.ProductionServices.Base
{
    public partial interface IProductionService
    {
        Task<VanBoard?> GetBoardByIdAsync(string id);
        VanBoard? GetBoardById(string id);
        IEnumerable<VanBoard> GetBoards(IEnumerable<string> ids);
        Task<IEnumerable<VanBoard>> GetBoardsAsync(IProgress<double> progress, IEnumerable<string> ids);

        public async Task<IEnumerable<VanBoard>> GetProductionLineBoardsAsync(IProgress<double> progress, bool includeCarParkVans) => await GetProductionLineBoardsAsync(progress, Enum.GetValues<VanModel>(), includeCarParkVans);
        public async Task<IEnumerable<VanBoard>> GetProductionLineBoardsAsync(IProgress<double> progress, IEnumerable<VanModel> vanTypes, bool includeCarParkVans)
        {
            List<VanBoard> boards = new List<VanBoard>();

            var productionBoards = await GetBoardsAsync(progress, ProductionVanIds(vanTypes, includeCarParkVans));

            boards.AddRange(productionBoards);

            return boards;
        }

        public IEnumerable<VanBoard> GetProductionLineBoards(bool includeCarParkVans)
        {
            List<VanBoard> boards = new List<VanBoard>();

            var productionBoards = GetBoards(ProductionVanIds(includeCarParkVans));

            boards.AddRange(productionBoards);

            return boards;
        }

        public async Task<IEnumerable<VanBoard>> GetVanByLocalHandoverDatesAsync(IProgress<double> progress, params DateTime[] dateTimes)
        {
            IEnumerable<string> ids = GetVanIdsByLocalHandoverDates(dateTimes);

            return await GetBoardsAsync(progress, ids);
        }

        Task<IEnumerable<VanBoard>> GetNextHandoverVansAsync(IProgress<double> progress, IEnumerable<VanModel> vanTypes, int limit) => GetBoardsAsync(progress, GetNextHandoverIds(vanTypes, limit));
        IEnumerable<VanBoard> GetNextHandoverVans(IEnumerable<VanModel> vanTypes, int limit) => GetBoards(GetNextHandoverIds(vanTypes, limit));
    }
}