namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService
{
    public async Task<IEnumerable<JobCard>> GetJobCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds)
    {
        double       report     = 0;
        List<string> enumerable = boardIds.ToList();
        foreach (string boardId in enumerable)
        {
            report += 100d / enumerable.Count();
            progress.Report(report);

            if (!_vanBoards.ContainsKey(boardId)) await GetBoardByIdAsync(boardId);
        }

        return GetJobCards(enumerable);
    }
        
    public IEnumerable<JobCard> GetJobCards(IEnumerable<string> boardIds)
    {
        List<JobCard> cards = [];

        foreach (JobCardObject card in _jobCards.Values.Where(x => boardIds.Contains(x.BoardId)))
        {
            JobCard? jobCard = GetJobCard(card);
            
            if (jobCard is not null)
                cards.Add(jobCard);
        }

        return cards.AsEnumerable();
    }

    public IEnumerable<JobCard> GetJobCards(IEnumerable<VanModel> vanTypes)
    {
        List<JobCard> cards = [];

        foreach (JobCardObject card in _jobCards.Values.Where(x => vanTypes.Contains(x.Name.ToVanType())))
        {
            JobCard? jobCard = GetJobCard(card);
            
            if (jobCard is not null)
                cards.Add(jobCard);
        }

        return cards.AsEnumerable();
    }

    public Check? GetCheck(string id)
    {
        if (!_checks.TryGetValue(id, out CheckObject? checkObject))
            return null;

        return new Check(checkObject);
    }

    public Checklist? GetChecklist(string id)
    {
        if (!_checkLists.TryGetValue(id, out ChecklistObject? checklistObject))
            return null;

        List<Check> checks = [];

        foreach (string checkId in checklistObject.CheckObjectIds)
        {
            if(_checks.TryGetValue(checkId, out var check))
                checks.Add(new Check(check));
        }

        return new Checklist(checklistObject.Name, checks);
    }

    public JobCard? GetJobCard(string id)
    {
        if (!_jobCards.TryGetValue(id, out JobCardObject? jobObject))
            return null;

        return GetJobCard(jobObject);
    }
}