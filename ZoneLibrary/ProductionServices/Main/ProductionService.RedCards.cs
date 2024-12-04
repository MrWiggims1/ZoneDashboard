namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService
{
    public RedCard? GetRedCard(string id)
    {
        if (!_redCards.TryGetValue(id, out RedCardObject? redObject))
            return null;

        return GetRedCard(redObject);
    }
    
    public YellowCard? GetYellowCard(string id)
    {
        if (!_yellowCards.TryGetValue(id, out RedCardObject? redCardObject))
            return null;

        return GetYellowCard(redCardObject);
    }
    
    public IEnumerable<RedCard> GetRedCards()
    {
        return _redCards.Values.Select(redCard => GetRedCard(redCard)).ToList();
    }
    
    public IEnumerable<YellowCard> GetYellowCards()
    {
        return _yellowCards.Values.Select(yellowCard => GetYellowCard(yellowCard)).ToList();
    }

    public IEnumerable<RedCard> GetRedCards(IEnumerable<VanModel> vanTypes)
    {
        List<RedCardObject> redCardObjects = new List<RedCardObject>();
        IEnumerable<string> boardNames     = ProductionVans.Keys.Where(x => vanTypes.Contains(x.ToVanType()));

        foreach (string name in boardNames)
        {
            redCardObjects.AddRange(_redCards.Values.Where(x => x.BoardId == ProductionVans[name].Id));
        }

        return redCardObjects.Select(redCard => GetRedCard(redCard));
    }

    public IEnumerable<YellowCard> GetYellowCards(IEnumerable<VanModel> vanTypes)
    {
        List<RedCardObject> yellowCardObjects = new List<RedCardObject>();
        IEnumerable<string> boardNames        = ProductionVans.Keys.Where(x => vanTypes.Contains(x.ToVanType()));

        foreach (string name in boardNames)
        {
            yellowCardObjects.AddRange(_yellowCards.Values.Where(x => x.BoardId == ProductionVans[name].Id));
        }

        return yellowCardObjects.Select(yellowCard => GetYellowCard(yellowCard));
    }
    
    public IEnumerable<RedCard> GetRedCards(IEnumerable<string> boardIds)
    {
        List<string> enumerable = boardIds.ToList();
            
        foreach (string boardId in enumerable.Where(boardId => !_vanBoards.ContainsKey(boardId)))
        {
            Log.Logger.Error("{id} has not been initialized, could not be added in non async task method.", boardId);
        }

        return _redCards.Values.Where(x => enumerable.Contains(x.BoardId)).Select(redCard => GetRedCard(redCard)).ToList();
    }
    
    public IEnumerable<YellowCard> GetYellowCards(IEnumerable<string> boardIds)
    {
        List<string> enumerable = boardIds.ToList();
            
        foreach (string boardId in enumerable.Where(boardId => !_vanBoards.ContainsKey(boardId)))
        {
            Log.Logger.Error("{id} has not been initialized, could not be added in non async task method.", boardId);
        }

        return _yellowCards.Values.Where(x => enumerable.Contains(x.BoardId)).Select(yellowCard => GetYellowCard(yellowCard)).ToList();
    }

    public async Task<IEnumerable<RedCard>> GetRedCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds)
    {
        List<string> enumerable = boardIds.ToList();

        await GetBoardsAsync(progress, enumerable);

        return _redCards.Values.Where(x => enumerable.Contains(x.BoardId)).Select(redCard => GetRedCard(redCard)).ToList();
    }

    public async Task<IEnumerable<YellowCard>> GetYellowCardsAsync(IProgress<double> progress, IEnumerable<string> boardIds)
    {
        List<string> enumerable = boardIds.ToList();

        await GetBoardsAsync(progress, enumerable);
        
        return _yellowCards.Values.Where(x => enumerable.Contains(x.BoardId)).Select(yellowCard => GetYellowCard(yellowCard)).ToList();
    }
        
    public Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds)
    {
        Dictionary<CardAreaOfOrigin, List<RedCard>> values = new Dictionary<CardAreaOfOrigin, List<RedCard>>();

        List<RedCardObject> redCardObjects = new List<RedCardObject>();

        IEnumerable<string> boardNames;

        if (boardIds != null)
            boardNames = ProductionVans.Where(x => boardIds.Contains(x.Value.Id) && vanTypes.Contains(x.Key.ToVanType())).Select(x => x.Key);

        else
            boardNames = ProductionVans.Keys.Where(x => vanTypes.Contains(x.ToVanType()));

        foreach (string name in boardNames)
        {
            redCardObjects.AddRange(_redCards.Values.Where(x => x.BoardId == ProductionVans[name].Id));
        }

        foreach (CardAreaOfOrigin area in Enum.GetValues<CardAreaOfOrigin>())
        {
            IEnumerable<RedCardObject> cards = redCardObjects.Where(x => x.AreaOfOrigin == area);

            List<RedCardObject> cardObjects = cards.ToList();
            if (cardObjects.Count == 0)
                continue;

            values.Add(area, new List<RedCard>());

            foreach (RedCardObject card in cardObjects)
            {
                values[area].Add(GetRedCard(card));
            }
        }

        return values;
    }

    public Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds)
    {
        Dictionary<RedFlagIssue, List<RedCard>> values = new Dictionary<RedFlagIssue, List<RedCard>>();

        List<RedCardObject> redCardObjects = new List<RedCardObject>();

        IEnumerable<string> boardNames;

        if (boardIds != null)
            boardNames = ProductionVans.Where(x => boardIds.Contains(x.Value.Id) && vanTypes.Contains(x.Key.ToVanType())).Select(x => x.Key);

        else
            boardNames = ProductionVans.Keys.Where(x => vanTypes.Contains(x.ToVanType()));

        foreach (string name in boardNames)
        {
            redCardObjects.AddRange(_redCards.Values.Where(x => x.BoardId == ProductionVans[name].Id));
        }

        foreach (RedFlagIssue type in Enum.GetValues<RedFlagIssue>())
        {
            List<RedCardObject> cards = redCardObjects.Where(x => x.RedFlagIssue == type).ToList();

            if (cards.Count == 0)
                continue;

            values.Add(type, new List<RedCard>());

            foreach (RedCardObject card in cards)
            {
                values[type].Add(GetRedCard(card));
            }
        }

        return values;
    }

    public SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds, DateTime startDate, DateTime endDate)
    {
        SortedDictionary<DateTime, List<RedCard>> values = new SortedDictionary<DateTime, List<RedCard>>();

        List<RedCardObject> redCardObjects = new List<RedCardObject>();

        IEnumerable<string> boardNames;

        if (boardIds != null)
            boardNames = ProductionVans.Where(x => boardIds.Contains(x.Value.Id) && vanTypes.Contains(x.Key.ToVanType())).Select(x => x.Key);

        else
            boardNames = ProductionVans.Keys.Where(x => vanTypes.Contains(x.ToVanType()));

        foreach (string name in boardNames)
        {
            redCardObjects.AddRange(_redCards.Values.Where(x => x.BoardId == ProductionVans[name].Id));
        }

        startDate = startDate.Date;
        endDate   = endDate.Date;

        foreach (RedCardObject redCard in redCardObjects)
        {
            if (!redCard.CreationDate.HasValue)
                continue;

            DateTime redCardDate = redCard.CreationDate.Value.LocalDateTime.Date;

            if (redCardDate < startDate || redCardDate > endDate)
                continue;

            if (!values.ContainsKey(redCardDate))
                values.Add(redCardDate, new List<RedCard>());

            values[redCardDate].Add(GetRedCard(redCard));
        }

        return values;
    }

}