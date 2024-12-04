namespace ZoneProductionLibrary.ProductionServices.Base
{
    public partial interface IProductionService
    {
        IEnumerable<RedCard> GetRedCards();
        Task<IEnumerable<RedCard>> GetRedCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds);
        IEnumerable<RedCard> GetRedCards(IEnumerable<string> boardIds);
        IEnumerable<RedCard> GetRedCards(IEnumerable<VanModel> vanTypes);
        RedCard? GetRedCard(string id);

        
        IEnumerable<YellowCard> GetYellowCards();
        Task<IEnumerable<YellowCard>> GetYellowCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds);
        IEnumerable<YellowCard> GetYellowCards(IEnumerable<string> boardIds);
        IEnumerable<YellowCard> GetYellowCards(IEnumerable<VanModel> vanTypes);
        YellowCard? GetYellowCard(string id);
        
        Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin() => GetRedCardsByAreaOfOrigin(Enum.GetValues<VanModel>(), null);
        Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(BoardFilterOptions filterOptions) => GetRedCardsByAreaOfOrigin(filterOptions.vanTypes, filterOptions.BoardIds);
        Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(IEnumerable<string>? boardIds) => GetRedCardsByAreaOfOrigin(Enum.GetValues<VanModel>(), boardIds);
        Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(IEnumerable<VanModel> vanTypes) => GetRedCardsByAreaOfOrigin(vanTypes, null);
        Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds);

        Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType() => GetRedCardsByRedFlagType(Enum.GetValues<VanModel>(), null);
        Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(BoardFilterOptions filterOptions) => GetRedCardsByRedFlagType(filterOptions.vanTypes, filterOptions.BoardIds);
        Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(IEnumerable<VanModel> vanTypes) => GetRedCardsByRedFlagType(vanTypes, null);
        Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(IEnumerable<string>? boardIds) => GetRedCardsByRedFlagType(Enum.GetValues<VanModel>(), boardIds);
        Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds);

        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate() => GetRedCardsByLocalDate(Enum.GetValues<VanModel>());
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(BoardFilterOptions filterOptions) => GetRedCardsByLocalDate(filterOptions.vanTypes, filterOptions.BoardIds);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(BoardFilterOptions filterOptions, DateTime startDate, DateTime endDate) => GetRedCardsByLocalDate(filterOptions.vanTypes, filterOptions.BoardIds, startDate, endDate);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(DateTime startDate, DateTime endDate) => GetRedCardsByLocalDate(Enum.GetValues<VanModel>(), null, startDate, endDate);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes) => GetRedCardsByLocalDate(vanTypes, null, DateTime.MinValue, DateTime.MaxValue);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes, DateTime startDate, DateTime endDate) => GetRedCardsByLocalDate(vanTypes, null, startDate, endDate);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<string>? boardIds) => GetRedCardsByLocalDate(Enum.GetValues<VanModel>(), boardIds, DateTime.MinValue, DateTime.MaxValue);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<string>? boardIds, DateTime startDate, DateTime endDate) => GetRedCardsByLocalDate(Enum.GetValues<VanModel>(), boardIds, startDate, endDate);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds) => GetRedCardsByLocalDate(Enum.GetValues<VanModel>(), boardIds, DateTime.MinValue, DateTime.MaxValue);
        SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds, DateTime startDate, DateTime endDate);
    }
}