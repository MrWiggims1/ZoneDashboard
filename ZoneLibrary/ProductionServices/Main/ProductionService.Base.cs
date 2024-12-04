using DBLibrary.Data;
using DBLibrary.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using PnP.Framework.Extensions;
using Serilog.Context;
using System.Diagnostics.CodeAnalysis;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetMemberOptions;
using TrelloDotNet.Model.Search;
using TrelloDotNet.Model.Webhook;
using ZoneProductionDashBoard;
using Comment = ZoneProductionLibrary.Models.Boards.Comment;

namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService : IProductionService
{
    private IVanIdData _vanIdDataDB;
    private ITrelloActionData _trelloActionDataDB;
    private TrelloClient _trelloClient    = null!;

    private          List<Webhook> _webhooks       = [];

    public static readonly string CCDashboardId   = "5f1a1f029e25dd741ebc3466";
    public static readonly string LineMoveBoardId = "6089f58c359e19533e9b7f1c";

    public List<ProductionDepartment> Gen2ProductionDepartments { get => _gen2ProductionDepartments; set => throw new NotImplementedException(); }
    private readonly List<ProductionDepartment> _gen2ProductionDepartments =
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


    public List<ProductionDepartment> ExpoProductionDepartments { get => _expoProductionDepartments; set => throw new NotImplementedException(); }
    private readonly List<ProductionDepartment> _expoProductionDepartments =
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



        
    public Dictionary<string, VanProductionInfo>      ProductionVans { get; } = new Dictionary<string, VanProductionInfo>();
    public ConcurrentDictionary<string, TrelloMember> Members        { get; } = new ConcurrentDictionary<string, TrelloMember>();

    internal          ConcurrentDictionary<string, CheckObject>     _checks      = new ConcurrentDictionary<string, CheckObject>();
    internal          ConcurrentDictionary<string, ChecklistObject> _checkLists  = new ConcurrentDictionary<string, ChecklistObject>();
    internal          ConcurrentDictionary<string, JobCardObject>   _jobCards    = new ConcurrentDictionary<string, JobCardObject>();
    internal          ConcurrentDictionary<string, RedCardObject>   _redCards    = new ConcurrentDictionary<string, RedCardObject>();
    internal          ConcurrentDictionary<string, RedCardObject>   _yellowCards = new ConcurrentDictionary<string, RedCardObject>();
    internal readonly ConcurrentDictionary<string, VanBoardObject>  _vanBoards   = new ConcurrentDictionary<string, VanBoardObject>();
    internal          ConcurrentDictionary<string, CommentObject>   _comments    = new ConcurrentDictionary<string, CommentObject>();

    public int VanCount => _vanBoards.Count;

    public int RedCardCount            => _redCards.Count;

    public int YellowCardCount                => _yellowCards.Count;
    
    public int JobCardCount                => _jobCards.Count;
    
    public int CheckCount                => _checks.Count;

    public ProductionService(IVanIdData vanIdData, ITrelloActionData trelloActionData)
    {
        _vanIdDataDB = vanIdData;
        _trelloActionDataDB = trelloActionData;
    }

    public async Task InitializeProductionService()
    {
        Log.Logger.Information("Initializing production service...");
            
        await StartTrelloService();
            
        List<CardAreaOfOrigin> otherAreaGen2 = new List<CardAreaOfOrigin>();
        List<CardAreaOfOrigin> otherAreaExpo = new List<CardAreaOfOrigin>();

        foreach (CardAreaOfOrigin area in Enum.GetValues<CardAreaOfOrigin>())
        {
            if (!_gen2ProductionDepartments.Any(x => x.AreaOfOrigins.Contains(area)))
                otherAreaGen2.Add(area);

            if (!_expoProductionDepartments.Any(x => x.AreaOfOrigins.Contains(area)))
                otherAreaExpo.Add(area);
        }

        if (otherAreaGen2.Count != 0)
            _gen2ProductionDepartments.Add(new ProductionDepartment("Other", otherAreaGen2));

        if (otherAreaExpo.Count != 0)
            _expoProductionDepartments.Add(new ProductionDepartment("Other", otherAreaExpo));

        await InitializeProductionInformation();

        if (!Directory.Exists(IProductionService.FileBasePath         + "attachments"))
            Directory.CreateDirectory(IProductionService.FileBasePath + "attachments");
    }

    private async Task StartTrelloService()
    {
        TrelloClientOptions clientOptions = new TrelloClientOptions
                                            {
                                                AllowDeleteOfBoards = false,
                                                AllowDeleteOfOrganizations = false,
                                                IncludeCustomFieldsInCardGetMethods = true,
                                                IncludeAttachmentsInCardGetMethods = false,
                                                MaxRetryCountForTokenLimitExceeded = 3
                                            };

        _trelloClient = new TrelloClient(DashboardConfig.TrelloApiKey , DashboardConfig.TrelloUserToken, clientOptions);
        Member member;

        try
        {
            member = await _trelloClient.GetTokenMemberAsync();
            Log.Logger.Information("Trello Successfully connected as {member}", member.Username);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Trello client could not connect");

            throw;
        }

        if (DashboardConfig.EnableWebhooks)
        {
            _webhooks = await _trelloClient.GetWebhooksForCurrentTokenAsync();

            Log.Logger.Information("{ActiveWebhookCount} webhooks active out of {webhookCount}", _webhooks.Count(x => x.Active), _webhooks.Count);
        }

        List<Organization> organisations = await _trelloClient.GetOrganizationsForMemberAsync(member.Id);

        foreach (Organization organization in organisations)
        {
            GetMemberOptions getMemberOptions = new GetMemberOptions
                                                {
                                                    MemberFields = new MemberFields(
                                                         MemberFieldsType.FullName,
                                                         MemberFieldsType.Username,
                                                         MemberFieldsType.AvatarUrl
                                                        )
                                                };

            List<Member> members = await _trelloClient.GetMembersOfOrganizationAsync(organization.Id, getMemberOptions);

            foreach (Member orgMember in members)
            {
                if (!Members.ContainsKey(orgMember.Id))
                {
                    TrelloMember newMember = new TrelloMember(orgMember, organization.Id);
                    Members.TryAdd(orgMember.Id, newMember);

                    Log.Logger.Debug("New member [{memberINfo}] added to production service.", newMember);
                }
            }
        }
    }

    private async Task InitializeProductionInformation()
    {
        List<Card>         lineMoveBoardCards = await _trelloClient.GetCardsOnBoardAsync(LineMoveBoardId, new GetCardOptions() {IncludeList = true});
        List<TrelloAction> lineMoveActions    = await _trelloClient.GetActionsOfBoardAsync(LineMoveBoardId, ["updateCard:idList"], 1000);
        lineMoveActions.AddRange(await _trelloClient.GetActionsOfBoardAsync(LineMoveBoardId, ["updateCard:idList"], 1000, before: lineMoveActions.MinBy(x => x.Date)!.Id));
        
        List<Card>         ccDashboardCards   = await _trelloClient.GetCardsOnBoardAsync(CCDashboardId, new GetCardOptions());

        IEnumerable<CachedTrelloAction> cachedActions = await _trelloActionDataDB.GetActions(CCDashboardId);
        List<TrelloAction> actionsToCache = [];
        List<TrelloAction> newActions;
        
        if (cachedActions.Count() == 0)
            newActions = await _trelloClient.GetActionsOfBoardAsync(CCDashboardId, ["updateCard"], 1000);

        else
            newActions = await _trelloClient.GetActionsOfBoardAsync(CCDashboardId, ["updateCard"], 1000, since: cachedActions.Last().ActionId);
        
        actionsToCache.AddRange(newActions);
        
        while (newActions.Count == 1000)
        {
            string lastId = newActions.Last().Id;
            if(cachedActions.Count() == 0)
                newActions = await _trelloClient.GetActionsOfBoardAsync(CCDashboardId, ["updateCard"], 1000, before: lastId);
                
            else
                newActions = await _trelloClient.GetActionsOfBoardAsync(CCDashboardId, ["updateCard"], 1000, before: lastId, since: cachedActions.Last().ActionId);
                
            actionsToCache.AddRange(newActions);
        }
        
        if (actionsToCache.Count > 0)
        {
            IEnumerable<TrelloAction> actionsWithDueDate = actionsToCache.Where(x => x.Data.Card.Due.HasValue);
            
            IEnumerable<CachedTrelloAction> returnedActions = await _trelloActionDataDB.InsertTrelloActions(actionsWithDueDate);

            cachedActions = returnedActions.Concat(cachedActions);
        }

        List<List>? lists = await _trelloClient.GetListsOnBoardAsync(LineMoveBoardId);

        lineMoveBoardCards = lineMoveBoardCards
                             .Where(x => 
                                        x.Name.ToLower().Contains("zsp") || 
                                         x.Name.ToLower().Contains("zpp") || 
                                         x.Name.ToLower().Contains("exp") || 
                                         x.Name.ToLower().Contains("zss")
                                         ).ToList();
        
        ccDashboardCards = ccDashboardCards.Where(x => 
                                                      x.Name.ToLower().Contains("zsp") || 
                                                      x.Name.ToLower().Contains("zpp") || 
                                                      x.Name.ToLower().Contains("exp") || 
                                                      x.Name.ToLower().Contains("zss")
                                                      ).ToList();

        List<string> tempBlockedNames = [];

        IEnumerable<VanID> storedVanIds = await _vanIdDataDB.GetIds();
        List<VanID> storedVanIdList = storedVanIds.ToList();

        foreach (Card card in lineMoveBoardCards)
        {
            using (LogContext.PushProperty("CardLink", "https://trello.com/c/" + card.Id))
            using (LogContext.PushProperty("CardName", card.Name))
            {
                if (TrelloUtil.TryGetVanName(card.Name, out _, out string formattedName))
                {
                    if (tempBlockedNames.Contains(formattedName))
                        continue;

                    if (ProductionVans.ContainsKey(formattedName))
                    {
                        ProductionVans.Remove(formattedName);
                        tempBlockedNames.Add(formattedName);

                        Log.Logger.Error("{name} found at least twice in line move board. Ignoring both until issue is resolved.", formattedName);
                        
                        continue;
                    }

                    VanID? vanId = storedVanIdList.SingleOrDefault(x => x.VanName == formattedName);
                    string idString = string.Empty;
                    string urlString = string.Empty;
                    
                    if(vanId is not null && vanId.Blocked)
                        continue;

                    if (vanId is null || string.IsNullOrEmpty(vanId.VanId) || string.IsNullOrEmpty(vanId.Url))
                    {
                        TimeSpan lastUpdated = DateTimeOffset.UtcNow - card.LastActivity.UtcDateTime;
                        (bool boardfound, VanID? vanId) search = await TrySearchForVanId(formattedName, lastUpdated);

                        if (!search.boardfound || search.vanId is null)
                            continue;

                        else
                        {
                            idString = search.vanId.VanId;
                            urlString = search.vanId.Url;
                        }
                    }
                    else
                    {
                        idString = vanId.VanId;
                        urlString = vanId.Url;
                    }

                    List<(DateTimeOffset date, IProductionPosition)> positionHistory = lineMoveActions.Where(x => x.Data.Card.Id == card.Id).ToPositionHistory(lists);

                    ProductionVans.Add(formattedName,
                                       new VanProductionInfo(idString, formattedName, urlString, positionHistory));
                    
                    Log.Logger.Debug("New van information added");
                }
                else
                {
                    Log.Logger.Debug("Does not represent a van, ignoring.");
                }
            }
        }

        Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", ProductionVans.Count);

        foreach (Card card in ccDashboardCards)
        {
            using (LogContext.PushProperty("CardLink", "https://trello.com/c/" + card.Id))
            using (LogContext.PushProperty("CardName", card.Name))
            {
                if (TrelloUtil.TryGetVanName(card.Name, out _, out string formattedName))
                {
                    if (!card.Due.HasValue)
                        continue;

                    if (ProductionVans.TryGetValue(formattedName, out VanProductionInfo? value))
                    {
                        IEnumerable<CachedTrelloAction> actions = cachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue);

                        foreach (CachedTrelloAction action in actions.OrderBy(x => x.DateOffset))
                        {
                            value.HandoverHistory.Add((action.DateOffset, action.DueDate!.Value));
                        }

                        if (card.Due.HasValue)
                            value.HandoverState =
                                card.DueComplete ? HandoverState.HandedOver : HandoverState.UnhandedOver;

                        Log.Logger.Debug("Added {handover} to {vanName} ({handoverStat})", card.Due.Value.LocalDateTime.Date.ToString("dd/MM/yy"), value.Name, value.HandoverState);
                    }
                }
                else
                {
                    Log.Logger.Debug("Does not represent a van, ignoring.");
                }
            }
        }

        Log.Logger.Information("{vanCount} handover dates added", ProductionVans.Count(x => x.Value.Handover.HasValue));

        int gen2PreProductionCount = ProductionVans.Count(x => x.Key.ToVanType().IsGen2() && x.Value.Position is PreProduction);
        int expoPreProductionCount = ProductionVans.Count(x => !x.Key.ToVanType().IsGen2() && x.Value.Position is PreProduction);

        int gen2ProductionCount = ProductionVans.Count(x => x.Value.Position is Gen2ProductionPosition);
        int expoProductionCount = ProductionVans.Count(x => x.Value.Position is ExpoProductionPosition);

        int gen2PostProductionCount = ProductionVans.Count(x => x.Key.ToVanType().IsGen2() && x.Value.Position is PostProduction);
        int expoPostProductionCount = ProductionVans.Count(x => !x.Key.ToVanType().IsGen2() && x.Value.Position is PostProduction);

        int gen2PostProductionUnhandedOverCount = ProductionVans.Count(x => x.Key.ToVanType().IsGen2() && x.Value.IsInCarPark);
        int expoPostProductionUnhandedOverCount = ProductionVans.Count(x => !x.Key.ToVanType().IsGen2() && x.Value.IsInCarPark);
        Log.Logger.Information("Gen 2: Pre-Production:{pre} - In Production:{prod} - Post Production:{post} ({yetToHandover} unhanded over)", gen2PreProductionCount, gen2ProductionCount, gen2PostProductionCount, gen2PostProductionUnhandedOverCount);
        Log.Logger.Information("EXPO: Pre-Production:{pre} - In Production:{prod} - Post Production:{post} ({yetToHandover} unhanded over)", expoPreProductionCount, expoProductionCount, expoPostProductionCount, expoPostProductionUnhandedOverCount);
    }

    public async Task CheckRequiredWebhooksActive()
    {
        _webhooks = await _trelloClient.GetWebhooksForCurrentTokenAsync();
        
        Webhook? ccDahsboardWebhook = _webhooks.SingleOrDefault(x => x.IdOfTypeYouWishToMonitor == CCDashboardId);
        
        if (ccDahsboardWebhook is null)
        {
            Webhook webhook = new Webhook("CC Dashboard Webhook", DashboardConfig.WebhookCallbackUrl, CCDashboardId);

            webhook.Active = true;

            await _trelloClient.AddWebhookAsync(webhook);
            
            _webhooks.Add(webhook);
            
            Log.Logger.Information("CC Dashboard webhook added.");
        }
        else if (!ccDahsboardWebhook.Active || ccDahsboardWebhook.CallbackUrl != DashboardConfig.WebhookCallbackUrl)
        {
            ccDahsboardWebhook.Active = true;
            ccDahsboardWebhook.CallbackUrl = DashboardConfig.WebhookCallbackUrl;

            await _trelloClient.UpdateWebhookAsync(ccDahsboardWebhook);
            
            Log.Logger.Information("CC Dashboard webhook updated.");
        }
            
        Webhook? lineMoveWebhook = _webhooks.SingleOrDefault(x => x.IdOfTypeYouWishToMonitor == LineMoveBoardId);
        
        if (lineMoveWebhook is null)
        {
            Webhook webhook = new Webhook("Line move board", DashboardConfig.WebhookCallbackUrl, LineMoveBoardId);

            webhook.Active = true;

            _webhooks.Add(webhook);
            
            await _trelloClient.AddWebhookAsync(webhook);
            
            Log.Logger.Information("Line move board webhook updated.");
        }
        else if (!lineMoveWebhook.Active || lineMoveWebhook.CallbackUrl != DashboardConfig.WebhookCallbackUrl)
        {
            lineMoveWebhook.Active = true;
            lineMoveWebhook.CallbackUrl = DashboardConfig.WebhookCallbackUrl;

            await _trelloClient.UpdateWebhookAsync(lineMoveWebhook);
            
            Log.Logger.Information("Line move board webhook updated.");
        }

        Member clientMember = await _trelloClient.GetTokenMemberAsync();
        IEnumerable<Organization> memberOrganisations
            = await _trelloClient.GetOrganizationsForMemberAsync(clientMember.Id);

        foreach (Organization org in memberOrganisations)
        {
            Webhook? webhook = _webhooks.FirstOrDefault(x => x.IdOfTypeYouWishToMonitor == org.Id);

            if (webhook is null)
            {
                webhook = new Webhook($"{org.Name} webhook", DashboardConfig.WebhookCallbackUrl, org.Id);

                webhook.Active = true;
                
                _webhooks.Add(webhook);

                await _trelloClient.AddWebhookAsync(webhook);
                
                Log.Logger.Information("Webhook added for {orgName} organisation", org.DisplayName);
            }
            else if (!webhook.Active || webhook.CallbackUrl != DashboardConfig.WebhookCallbackUrl)
            {
                webhook.Active = true;
                webhook.CallbackUrl = DashboardConfig.WebhookCallbackUrl;

                await _trelloClient.UpdateWebhookAsync(webhook);
                
                Log.Logger.Information("Webhook updated for {orgName} organisation", org.DisplayName);
            }
        }
    }

    public async Task<(bool boardfound, VanID? vanId)> TrySearchForVanId(string name, TimeSpan? age = null)
    {
        if (_trelloClient is null)
            throw new Exception("Trello Client has not been initialized.");

        VanID? vanId = await _vanIdDataDB.GetId(name);

        if (vanId is null)
        {
            vanId = new VanID(name);
            await _vanIdDataDB.InsertVanId(vanId);
        }
        else
        {
            if (vanId.Blocked)
                return (false, null);

            if (!string.IsNullOrEmpty(vanId.VanId) && !string.IsNullOrEmpty(vanId.Url))
                return (true, null);
        }

        SearchRequest searchRequest = new SearchRequest(name)
                                      {
                                          SearchCards = false,
                                          BoardFields = new SearchRequestBoardFields("name", "closed", "url", "shorturl")
                                      };

        SearchResult searchResults = await _trelloClient.SearchAsync(searchRequest);

        List<Board> results = searchResults.Boards.Where(x => !x.Closed).ToList();

        if (results.Count() > 1)
        {
            Log.Logger.Error("Multiple Boards found for van {name}, not adding to cache - {urlList}", name, string.Join(", ", results.Select(x => $"https://trello.com/b/{x.Id}/")));

            return (false, null);
        }

        if (results.Count() == 0)
        {
            if (age.HasValue && age > TimeSpan.FromDays(90))
            {
                vanId.Blocked = true;
                await _vanIdDataDB.UpdateVanId(vanId);
                
                Log.Logger.Warning("No trello search result for {name}, blocking van from future searches", name);
            }
            else
                Log.Logger.Warning("No trello search result for {name}", name);

            
            return (false, null);
        }

        vanId.VanId = results.First().Id;
        vanId.Url = results.First().Url;
        
        await _vanIdDataDB.UpdateVanId(vanId);

        var id = await _vanIdDataDB.GetId(name);
        
        return (true, id);
    }

    public IEnumerable<TrelloMember> GetTrelloMembers(IEnumerable<string> memberIds)
    {
        List<TrelloMember> members = new List<TrelloMember>();

        foreach (string memberId in memberIds)
        {
            if (Members.TryGetValue(memberId, out TrelloMember? member))
                members.Add(member);
            else
            {
                KeyNotFoundException notFound = new KeyNotFoundException($"Could not find member with id:{memberId}");
                Log.Logger.Error(notFound, "Id Not found");
            }
        }

        return members;
    }

    public IEnumerable<Comment> GetComments(IEnumerable<string> commentIds)
    {
        List<Comment> comments = new List<Comment>();

        foreach (string commentId in commentIds)
        {
            if (_comments.TryGetValue(commentId, out CommentObject? commentObject))
            {
                if (Members.TryGetValue(commentObject.CreatorMemberId, out TrelloMember? member))
                    comments.Add(new Comment(commentObject, member));
                else
                    Log.Logger.Error("Could not find member with id:{memberId}", commentObject.CreatorMemberId);
            }
            else
            {
                KeyNotFoundException notFound = new KeyNotFoundException($"Could not find comment with id:{commentId}");
                Log.Logger.Error(notFound, "Id Not found");
            }
        }

        return comments;
    }

    private VanBoard GetBoardFromObject(VanBoardObject boardObject)
    {
        return new VanBoard(
                            boardObject,
                            boardObject.Name.ToVanType().IsGen2()
                                ? Gen2ProductionDepartments
                                : ExpoProductionDepartments,
                            this,
                            ProductionVans[boardObject.Name].PositionHistory);
    }

    private RedCard GetRedCard(RedCardObject redCardObject)
    {
        return new RedCard(redCardObject, ProductionVans.Values.Single(x => x.Id == redCardObject.BoardId),
                           GetTrelloMembers(redCardObject.MemberIds), GetComments(redCardObject.CommentIds));
    }

    private YellowCard GetYellowCard(RedCardObject redCardObject)
    {
        return new YellowCard(redCardObject, ProductionVans.Values.Single(x => x.Id == redCardObject.BoardId), GetComments(redCardObject.CommentIds));
    }

    private JobCard? GetJobCard(JobCardObject cardObject)
    {
        if (ProductionVans.Values.Single(x => x.Id == cardObject.BoardId).VanModel.IsGen2())
        {
            if (Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.JobListName,
                                                          cardObject.TrelloListName, out var pos) && pos is not null)
            {
                return new JobCard(cardObject, this, pos, GetComments(cardObject.CommentIds));
            }
        }
        else
        {
            if (ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.JobListName,
                                                          cardObject.TrelloListName, out var pos) && pos is not null)
            {
                return new JobCard(cardObject, this, pos, GetComments(cardObject.CommentIds));
            }
        }

        return null;
    }
}