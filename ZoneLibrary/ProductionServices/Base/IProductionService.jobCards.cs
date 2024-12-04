namespace ZoneProductionLibrary.ProductionServices.Base
{
    public partial interface IProductionService
    {
        /// <summary>
        /// Will check to see if data is not cached and request the data
        /// </summary>
        Task<IEnumerable<JobCard>> GetJobCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds);
        /// <summary>
        /// Will check to see if data is not cached and request the data
        /// </summary>
        Task<IEnumerable<JobCard>> GetJobCardsAsync(IProgress<double> progress, string boardId) => GetJobCardsAsync(progress, [boardId]);
        /// <summary>
        /// Will NOT check to see if data is not cached
        /// </summary>
        IEnumerable<JobCard> GetJobCards(IEnumerable<string> boardIds);
        /// <summary>
        /// Will NOT check to see if data is not cached
        /// </summary>
        IEnumerable<JobCard> GetJobCards(string boardId) => GetJobCards([boardId]);
        IEnumerable<JobCard> GetJobCards(IEnumerable<VanModel> vanTypes);

        Check? GetCheck(string id);
        Checklist? GetChecklist(string id);
        JobCard? GetJobCard(string id);
    }
}