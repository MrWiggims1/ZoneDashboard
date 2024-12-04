using DBLibrary.Data;
using DBLibrary.Models;
using System.Collections.Concurrent;
using Serilog.Context;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Webhook;
using ZoneProductionDashBoard;

namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService
{
    private readonly ConcurrentDictionary<string, Task<VanBoard?>> _currentBoardTasks = [];

    public async Task<VanBoard?> GetBoardByIdAsync(string id)
    {
        VanProductionInfo productionInfo;

        if (ProductionVans.Values.Any(x => x.Id == id))
            productionInfo = ProductionVans.Values.Single(x => x.Id == id);
        else
            return null;

        if (_vanBoards.ContainsKey(id)) 
            return GetBoardFromObject(_vanBoards[productionInfo.Id]);
        
        if (_currentBoardTasks.TryGetValue(id, out Task<VanBoard?>? existingTask))
        {
            await Task.WhenAll([existingTask]);

            return existingTask.Result;
        }

        Task<VanBoard?> newTask = _GetBoardAsyncById(id);
        
        _currentBoardTasks.TryAdd(id, newTask);

        await newTask.WaitAsync(cancellationToken: default);

        await Task.Delay(100);
        
        _currentBoardTasks.TryRemove(id, out _);
        
        return newTask.Result;
    }
    
    private async Task<VanBoard?> _GetBoardAsyncById(string id) // TODO: add system to prevent rate limiting
    {
        VanProductionInfo productionInfo = ProductionVans.Values.Single(x => x.Id == id);

        if (_vanBoards.ContainsKey(id)) 
            return GetBoardFromObject(_vanBoards[productionInfo.Id]);

        GetCardOptions getCardOptions = new GetCardOptions
                                        {
                                            IncludeChecklists       = true,
                                            ChecklistFields         = ChecklistFields.All,
                                            IncludeList             = true,
                                            IncludeBoard            = true,
                                            BoardFields             = new BoardFields(BoardFieldsType.Name),
                                            IncludeCustomFieldItems = true,
                                            IncludeAttachments      = GetCardOptionsIncludeAttachments.True,
                                            CardFields = new CardFields(
                                                                        CardFieldsType.Name,
                                                                        CardFieldsType.Position,
                                                                        CardFieldsType.ChecklistIds,
                                                                        CardFieldsType.Closed,
                                                                        CardFieldsType.ListId,
                                                                        CardFieldsType.MemberIds)
                                        };
        
        using (LogContext.PushProperty("BoardLink", "https://trello.com/b/" + productionInfo.Id))
        using (LogContext.PushProperty("VanName", productionInfo.Name))
        {
            List<Card> cards;
            try
            { 
                cards = await _trelloClient.GetCardsOnBoardAsync(productionInfo.Id, getCardOptions);
            }
            catch (Exception ex) //TODO: Handle "TrelloDotNet.Model.TrelloApiException: The requested resource was not found" exception
            {
                if (ex is TrelloApiException && ex.Message == "invalid id")
                {
                    Log.Logger.Warning("{vanName} board id no longer exists. Refreshing Cached id.", productionInfo.Name);

                    await _vanIdDataDB.DeleteVanId(productionInfo.Name);

                    (bool boardfound, VanID? vanId) newId = await TrySearchForVanId(productionInfo.Name);

                    if (!newId.boardfound || newId.vanId is null)
                        throw new Exception("Board could not be found");
                    
                    ProductionVans[productionInfo.Name].Id = newId.vanId.VanId;
                    ProductionVans[productionInfo.Name].Url = newId.vanId.Url;
                    id = newId.vanId.VanId;
                    cards = await _trelloClient.GetCardsOnBoardAsync(newId.vanId.VanId, getCardOptions);
                }
                else
                {
                    throw;
                }
            }
            
            List<CustomField> customFields = await _trelloClient.GetCustomFieldsOnBoardAsync(productionInfo.Id);

            IEnumerable<CachedTrelloAction> cachedActions = await _trelloActionDataDB.GetActions(id);
            
            List<TrelloAction> actionsToCache = [];
            
            List<TrelloAction> newActions;
                
            if(cachedActions.Count() == 0)
                newActions = await _trelloClient.GetActionsOfBoardAsync(id, ITrelloActionData.ActionFilters, 1000);
            
            else
                newActions = await _trelloClient.GetActionsOfBoardAsync(id, ITrelloActionData.ActionFilters, 1000, since: cachedActions.Last().ActionId);

            actionsToCache.AddRange(newActions);
            
            while (newActions.Count == 1000)
            {
                string lastId = newActions.Last().Id;
                if(cachedActions.Count() == 0)
                    newActions = await _trelloClient.GetActionsOfBoardAsync(id, ITrelloActionData.ActionFilters, 1000, before: lastId);
                
                else
                    newActions = await _trelloClient.GetActionsOfBoardAsync(id, ITrelloActionData.ActionFilters, 1000, before: lastId, since: cachedActions.Last().ActionId);
                
                actionsToCache.AddRange(newActions);
            }

            if (actionsToCache.Count > 0)
            {
                var returnedActions = await _trelloActionDataDB.InsertTrelloActions(actionsToCache);

                cachedActions = returnedActions.Concat(cachedActions);
            }

            VanBoardObject boardObject = new VanBoardObject(this, 
                                                            productionInfo.Id,
                                                            ProductionVans.Single(x => x.Value == productionInfo).Key,
                                                            cards, 
                                                            customFields, 
                                                            cachedActions.ToList(),
                                                            productionInfo.Handover);

            if (!_vanBoards.TryAdd(productionInfo.Id, boardObject))
                throw new Exception("Unable to add van to dictionary.");
            
            BoardUpdated?.Invoke(this, new BoardUpdateInfo(id));
            
            if (DashboardConfig.EnableWebhooks)
            {
                List<Webhook> existingWebhooks = _webhooks.Where(x => x.IdOfTypeYouWishToMonitor == id).ToList();

                if (existingWebhooks.Count != 0)
                {
                    bool    webhookUpdated = false;
                    Webhook webhook        = existingWebhooks.First();

                    if (!webhook.Active)
                    {
                        webhookUpdated = true;
                        webhook.Active = true;
                    }

                    if (webhook.CallbackUrl != DashboardConfig.WebhookCallbackUrl)
                    {
                        webhookUpdated      = true;
                        webhook.CallbackUrl = DashboardConfig.WebhookCallbackUrl;
                    }

                    if (webhookUpdated)
                    {
                        Webhook updatedWebhook = await _trelloClient.UpdateWebhookAsync(webhook);

                        _webhooks.Remove(
                            _webhooks.First(x => x.IdOfTypeYouWishToMonitor == webhook.IdOfTypeYouWishToMonitor));
                        _webhooks.Add(updatedWebhook);
                        
                        Log.Logger.Debug("Webhook:[{webhookId} {descr}] updated", webhook.Id, webhook.Description);
                    }
                    else
                    {
                        Log.Logger.Debug("Webhook already active and up to date {id}: {descr}", webhook.Id,
                                         webhook.Description);
                    }
                }
                else
                {
                    Webhook newWebhook = new Webhook($"{boardObject.Name} {boardObject.Id}", DashboardConfig.WebhookCallbackUrl, id);
                    Webhook webhook    = await _trelloClient.AddWebhookAsync(newWebhook);
                    _webhooks.Add(webhook);

                    Log.Logger.Information("New Webhook added {id}: {descr}", webhook.Id, webhook.Description);
                }
            }

            Log.Logger.Information("New van board loaded {name}", boardObject.Name);

            return GetBoardFromObject(boardObject);
            
        }
    }
        
    public VanBoard? GetBoardById(string id)
    {
        VanProductionInfo productionInfo;

        if (ProductionVans.Values.Any(x => x.Id == id))
            productionInfo = ProductionVans.Values.Single(x => x.Id == id);
        else
            return null;

        if (_vanBoards.ContainsKey(id))
            return GetBoardFromObject(_vanBoards[productionInfo.Id]);
        return null;
    }


    public async Task<IEnumerable<VanBoard>> GetBoardsAsync(IProgress<double> progress, IEnumerable<string> ids)
    {
        ConcurrentBag<VanBoard> boards       = new ConcurrentBag<VanBoard>();
        double                   report       = 0;
        List<string>             idEnumerable = ids.ToList();

        ParallelOptions parallelOptions = new ParallelOptions
                                          {
                                              MaxDegreeOfParallelism = 3,
                                              CancellationToken      = default
                                          };

        await Parallel.ForEachAsync(idEnumerable, parallelOptions, async (id, _) =>
                                                                   {
                                                                       VanBoard? board =
                                                                           await GetBoardByIdAsync(id);

                                                                       report += 100d / idEnumerable.Count;
                                                                       progress.Report(report);

                                                                       if (board is null)
                                                                           throw new Exception("Null board received");

                                                                       boards.Add(board);
                                                                   });

        return boards;
    }
        
    public IEnumerable<VanBoard> GetBoards(IEnumerable<string> ids)
    {
        List<VanBoard> boards = new List<VanBoard>();

        foreach (string id in ids)
        {
            VanBoard? board = GetBoardById(id);

            if (board is null)
            {
                if (!_currentBoardTasks.ContainsKey(id))
                {
                    Log.Logger.Warning("{name}: {id} has not been initialized. Retrieving as background task.", ProductionVans.Values.Single(x => x.Id == id).Name, id);
                    
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    GetBoardByIdAsync(id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                
            }
            else
                boards.Add(board);
        }

        return boards;
    }
}