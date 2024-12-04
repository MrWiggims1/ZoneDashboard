using DBLibrary.Models;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetCardOptions;
using ZoneProductionLibrary.Models.UpdateData;

namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService
{
    public static event EventHandler<BoardUpdateInfo>? BoardUpdated;
    public static event EventHandler<string>? VanAddedToProduction;

    public void UpdateCheck(object? sender, CheckUpdatedData e)
    {
        if(!_jobCards.ContainsKey(e.CardId))
            return;
        
        if (_checks.TryGetValue(e.CheckId, out CheckObject? check))
        {
            if (check.CheckListId != e.CheckListId)
            {
                if (_checkLists.TryGetValue(check.CheckListId, out ChecklistObject? ogChecklist))
                    ogChecklist.RemoveCheckId(e.CheckId);

                if (_checkLists.TryGetValue(e.CheckListId, out ChecklistObject? newCheckList))
                    newCheckList.AddCheckId(e.CheckId);

                check.UpdateCheckListId(e.CheckListId);

                Log.Logger.Debug("Check:{checkId} moved to new checklist:{checkListId}", e.CheckId, e.CheckListId);
            }

            check.UpdateStatus(e.IsChecked, e.DateUpdated);
            check.UpdateName(e.CheckName);

            Log.Logger.Debug("Check:{checkId} Updated", e.CheckId);

            BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
        }
        else
        {
            Log.Logger.Warning("Check:{id} not found adding to card:{cardId}", e.CheckId, e.CardId);
            CreateCheck(this, e);
        }
    }

    public void DeleteCheck(object? sender, CheckDeletedData e)
    {
        if (_jobCards.ContainsKey(e.CardId))
        {
            if (TryRemoveCheck(e.CheckId))
            {
                Log.Logger.Debug("Check:{checkId} deleted.", e.CheckId);
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
            }
        }
    }

    public void CreateCheck(object? sender, CheckUpdatedData e)
    {
        if (!_jobCards.ContainsKey(e.CardId))
            return;
        
        if (_checkLists.TryGetValue(e.CheckListId, out ChecklistObject? list))
        {
            _checks.TryAdd(e.CheckId, new CheckObject(e));

            list.AddCheckId(e.CheckId);

            Log.Logger.Debug("New Check:{checkId} created added to CheckList:{checkList}", e.CheckId, e.CheckListId);

            BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
        }
        else
        {
            Log.Error("Could not find check list: {checklistId} on card. Data will not be up to date", e.CheckListId);
            // TODO: update checklist
        }
    }

    public void DeleteCheckList(object? sender, CheckListDeletedData e)
    {
        if (_jobCards.ContainsKey(e.CardId))
        {
            if (TryRemoveCheckList(e.CheckListId))
            {
                Log.Logger.Debug("CheckList:{checkListId} Deleted from card:{cardId}", e.CheckListId, e.CardId);
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
            }
        }
    }

    public void CreateCheckList(object? sender, CheckListCreatedData e)
    {
        if (_jobCards.TryGetValue(e.CardId, out JobCardObject? jobCard))
        {
            _checkLists.TryAdd(e.CheckListId, new ChecklistObject(e));
            jobCard.AddChecklistId(e.CheckListId);

            Log.Logger.Debug("New Checklist created:{checkListId} on card:{cardId}", e.CheckListId, e.CardId);

            BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
        }
    }

    public async void CopyCheckList(object? sender, CheckListCreatedData e)
    {
        if (_jobCards.TryGetValue(e.CardId, out _))
        {
            TrelloDotNet.Model.Checklist? trelloChecklist = await _trelloClient.GetChecklistAsync(e.CheckListId);

            if (trelloChecklist == null)
                return;

            if (_checkLists.TryGetValue(e.CheckListId, out ChecklistObject? checklist))
                foreach (ChecklistItem? trelloCheck in trelloChecklist.Items)
                {
                    _checks.TryAdd(trelloCheck.Id, new CheckObject(e.BoardId, trelloCheck, DateTimeOffset.Now));
                    checklist.AddCheckId(trelloCheck.Id);

                    Log.Logger.Debug("Check:{checkId} copied into Checklist{checklistId}", trelloCheck.Id,
                                     e.CheckListId);
                }

            BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));

            Log.Logger.Debug("Checklist:{checkListId} copied on card:{cardId}", e.CheckListId, e.CardId);
        }
    }

    public void UpdateCheckList(object? sender, CheckListUpdatedData e)
    {
        if(!_jobCards.ContainsKey(e.CardId))
            return;
        
        if (_checkLists.TryGetValue(e.CheckListId, out ChecklistObject? checklist))
        {
            checklist.UpdateName(e.CheckListName);
            BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));

            Log.Logger.Debug("Checklist:{checkListId} Updated", e.CheckListId);
        }
        else
        {
            Log.Error("Could not find check list: {checklistId} on card. Data will not be up to date", e.CheckListId);
            // TODO: update checklist
        }
    }

    public async void CreateCard(object? sender, CardUpdatedData e)
    {
        if (_redCards.ContainsKey(e.CardId) || _jobCards.ContainsKey(e.CardId) || _yellowCards.ContainsKey(e.CardId))
        {
            UpdateCard(sender, e);
        }

        if (_vanBoards.TryGetValue(e.BoardId, out VanBoardObject? boardObject))
        {
            GetCardOptions options = new GetCardOptions()
            {
                IncludeCustomFieldItems = true,
                ActionsTypes
                    = new ActionTypesToInclude("commentCard", "updateCustomFieldItem", "createCard",
                                               "updateCheckItemStateOnCard"),
                IncludeAttachments = GetCardOptionsIncludeAttachments.True,
                IncludeChecklists = true,
                ChecklistFields = ChecklistFields.All,
                IncludeList = true,
                IncludeBoard = true
            };

            Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, options);

            if (trelloCard.Closed)
                return;

            List<CustomField>? customFields = await _trelloClient.GetCustomFieldsOnBoardAsync(e.BoardId);

            bool boardUpdated = false;

            CardType cardType = VanBoardObject.GetCardType(trelloCard.Name, trelloCard.List.Name);

            if (cardType == CardType.RedCard)
            {
                IEnumerable<string> commentIds = VanBoardObject.AddComments(
                    this,
                    trelloCard.Actions.Where(x => x.Type == "commentCard").Select(x => new CachedTrelloAction(x))
                        .ToList());

                string? creatorMemberId
                    = trelloCard.Actions.SingleOrDefault(x => x.Type == "createCard")?.MemberCreatorId;

                RedCardObject newCard = new RedCardObject(trelloCard, customFields,
                                                          trelloCard.Actions.Select(x => new CachedTrelloAction(x)),
                                                          commentIds, creatorMemberId);

                _redCards.TryAdd(e.CardId, newCard);

                if (!boardObject.RedCardIds.Contains(e.CardId))
                {
                    boardObject.RedCardIds.Add(e.CardId);
                    Log.Logger.Debug("New Red Card added to {boardName}:{boardId}", boardObject.Name, boardObject.Id);
                }
                else
                    Log.Logger.Debug("Red Card updated on {boardName}:{boardId}", boardObject.Name, boardObject.Id);

                boardUpdated = true;
            }
            else if (cardType == CardType.YellowCard)
            {
                IEnumerable<string> commentIds = VanBoardObject.AddComments(
                    this,
                    trelloCard.Actions.Where(x => x.Type == "commentCard").Select(x => new CachedTrelloAction(x))
                        .ToList());

                string? creatorMemberId
                    = trelloCard.Actions.SingleOrDefault(x => x.Type == "createCard")?.MemberCreatorId;

                RedCardObject newCard = new RedCardObject(trelloCard, customFields,
                                                          trelloCard.Actions.Select(x => new CachedTrelloAction(x)),
                                                          commentIds, creatorMemberId);

                _yellowCards.TryAdd(e.CardId, newCard);

                if (!boardObject.YellowCardIds.Contains(e.CardId))
                {
                    boardObject.YellowCardIds.Add(e.CardId);
                    Log.Logger.Debug("New Yellow Card added to {boardName}:{boardId}", boardObject.Name,
                                     boardObject.Id);
                }
                else
                    Log.Logger.Debug("Yellow Card updated on {boardName}:{boardId}", boardObject.Name, boardObject.Id);

                boardUpdated = true;
            }
            else if (cardType == CardType.JobCard &&
                     TrelloUtil.ToCardAreaOfOrigin(trelloCard, customFields) != CardAreaOfOrigin.Unknown)
            {
                IEnumerable<string> commentIds = VanBoardObject.AddComments(
                    this,
                    trelloCard.Actions.Where(x => x.Type == "commentCard").Select(x => new CachedTrelloAction(x))
                        .ToList());

                JobCardObject newCard
                    = new JobCardObject(this, trelloCard, customFields,
                                        trelloCard.Actions.Select(x => new CachedTrelloAction(x)), commentIds);

                _jobCards.TryAdd(e.CardId, newCard);

                if (!boardObject.JobCardIds.Contains(e.CardId))
                {
                    boardObject.JobCardIds.Add(e.CardId);
                    Log.Logger.Debug("New Job Card added to {boardName}:{boardId}", boardObject.Name, boardObject.Id);
                }
                else
                    Log.Logger.Debug("Job Card updated on {boardName}:{boardId}", boardObject.Name, boardObject.Id);

                boardUpdated = true;
            }

            if (boardUpdated)
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
        }
    }

    public void DeleteCard(object? sender, CardUpdatedData e)
    {
        if(_vanBoards.ContainsKey(e.BoardId))
        {
            if (TryRemoveCard(e.CardId, out CardType cardType))
            {
                if (cardType == CardType.JobCard)
                {
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
                    Log.Logger.Debug("Job Card deleted from {cardName}:{boardId}", e.CardName, e.BoardId);
                }

                if (cardType == CardType.RedCard)
                {
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));
                    Log.Logger.Debug("Red Card deleted from {cardName}:{boardId}", e.CardName, e.BoardId);
                }

                if (cardType == CardType.YellowCard)
                {
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.YellowCard, e.CardId));
                    Log.Logger.Debug("Yellow Card deleted from {cardName}:{boardId}", e.CardName, e.BoardId);
                }

            }
        }
    }

    public void UpdateCard(object? sender, CardUpdatedData e)
    {
        if (_vanBoards.ContainsKey(e.BoardId))
        {
            TryRemoveCard(e.CardId);
            CreateCard(sender, e);
        }
    }

    public void MemberAddedToCard(object? sender, MemberAddedToCardData e)
    {
        if (_redCards.TryGetValue(e.CardId, out RedCardObject? card))
        {
            if (!card.MemberIds.Contains(e.MemberId))
            {
                card.MemberIds.Add(e.MemberId);
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));
            }
        }
    }
    
    public void MemberRemovedFromCard(object? sender, MemberAddedToCardData e)
    {
        if (_redCards.TryGetValue(e.CardId, out RedCardObject? card))
        {
            if (card.MemberIds.Contains(e.MemberId))
            {
                card.MemberIds.Remove(e.MemberId);
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));
            }
        }
    }

    public async void UpdateCustomFieldItems(object? sender, CardUpdatedData e)
    {
        if (_vanBoards.TryGetValue(e.BoardId, out _))
        {
            GetCardOptions options = new GetCardOptions()
            {
                IncludeCustomFieldItems = true,
                ActionsTypes = new ActionTypesToInclude("updateCustomFieldItem"),
                IncludeList = true
            };

            Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, options);
            List<CustomField>? customFields = await _trelloClient.GetCustomFieldsOnBoardAsync(e.BoardId);

            if (_jobCards.TryGetValue(e.CardId, out JobCardObject? jobCard))
            {
                if (TrelloUtil.ToCardAreaOfOrigin(trelloCard, customFields) == CardAreaOfOrigin.Unknown)
                {
                    DeleteCard(null, e);
                    return;
                }

                jobCard.UpdateCustomFieldsItems(trelloCard, customFields,
                                                trelloCard.Actions.Select(x => new CachedTrelloAction(x)));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
            }
            else if (_redCards.TryGetValue(e.CardId, out RedCardObject? redCard))
            {
                redCard.UpdateCustomFieldsItems(trelloCard, customFields,
                                                trelloCard.Actions.Select(x => new CachedTrelloAction(x)));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));
            }
            else if (_yellowCards.TryGetValue(e.CardId, out RedCardObject? yellowCard))
            {
                yellowCard.UpdateCustomFieldsItems(trelloCard, customFields,
                                                   trelloCard.Actions.Select(x => new CachedTrelloAction(x)));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.YellowCard, e.CardId));
            }
            else if (VanBoardObject.GetCardType(trelloCard.Name, trelloCard.List.Name) == CardType.JobCard &&
                     TrelloUtil.ToCardAreaOfOrigin(trelloCard, customFields) != CardAreaOfOrigin.Unknown)
            {
                CreateCard(null, e);
            }
        }
    }

    public async void UpdateCommentsOnCard(object? sender, CardUpdatedData e)
    {
        if (_vanBoards.TryGetValue(e.BoardId, out _))
        {
            if (_jobCards.TryGetValue(e.CardId, out JobCardObject? jobCard))
            {
                GetCardOptions options = new GetCardOptions()
                {
                    ActionsTypes = new ActionTypesToInclude("commentCard")
                };

                Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, options);
                
                foreach (string commentId in jobCard.CommentIds)
                {
                    _comments.TryRemove(commentId, out _);
                }

                jobCard.CommentIds = new List<string>();

                foreach (TrelloAction action in trelloCard.Actions)
                {
                    if (_comments.TryAdd(action.Id, new CommentObject(new CachedTrelloAction(action))))
                    {
                        jobCard.CommentIds.Add(action.Id);
                    }
                }
            
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));

                Log.Logger.Debug("Card comments updated {cardName}:{boardId}", e.CardName, e.BoardId);
            }

            else if (_redCards.TryGetValue(e.CardId, out RedCardObject? redCard))
            {
                GetCardOptions options = new GetCardOptions()
                {
                    ActionsTypes = new ActionTypesToInclude("commentCard")
                };

                Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, options);
                
                foreach (string commentId in redCard.CommentIds)
                {
                    _comments.TryRemove(commentId, out _);
                }

                redCard.CommentIds = new List<string>();

                foreach (TrelloAction action in trelloCard.Actions)
                {
                    if (_comments.TryAdd(action.Id, new CommentObject(new CachedTrelloAction(action))))
                    {
                        redCard.CommentIds.Add(action.Id);
                    }
                }
            
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));

                Log.Logger.Debug("Card comments updated {cardName}:{boardId}", e.CardName, e.BoardId);
            }

            else if (_yellowCards.TryGetValue(e.CardId, out RedCardObject? yellowCard))
            {

                GetCardOptions options = new GetCardOptions()
                {
                    ActionsTypes = new ActionTypesToInclude("commentCard")
                };

                Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, options);
                
                foreach (string commentId in yellowCard.CommentIds)
                {
                    _comments.TryRemove(commentId, out _);
                }

                yellowCard.CommentIds = new List<string>();

                foreach (TrelloAction action in trelloCard.Actions)
                {
                    if (_comments.TryAdd(action.Id, new CommentObject(new CachedTrelloAction(action))))
                    {
                        yellowCard.CommentIds.Add(action.Id);
                    }
                }
            
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.YellowCard, e.CardId));

                Log.Logger.Debug("Card comments updated {cardName}:{boardId}", e.CardName, e.BoardId);
            }
        }
    }

    public void AttachmentAdded(object? sender, AttachmentAddedData e)
    {
        if (_vanBoards.TryGetValue(e.BoardId, out VanBoardObject? van))
        {
            if (_jobCards.TryGetValue(e.CardId, out JobCardObject? jobCard))
            {
                jobCard.Attachments.Add(new AttachmentInfo(e));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));
            }
            
            else if (_redCards.TryGetValue(e.CardId, out RedCardObject? redCard))
            {
                redCard.Attachments.Add(new AttachmentInfo(e));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));
            }
            
            else if (_yellowCards.TryGetValue(e.CardId, out RedCardObject? yellowCard))
            {
                yellowCard.Attachments.Add(new AttachmentInfo(e));
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.YellowCard, e.CardId));
            }
        }
    }
    
    public void AttachmentDeleted(object? sender, AttachmentRemovedData e)
    {
        if (_vanBoards.TryGetValue(e.BoardId, out VanBoardObject? van))
        {
            bool updated = false;
            
            if (_jobCards.TryGetValue(e.CardId, out JobCardObject? jobCard))
            {
                AttachmentInfo? attachment = jobCard.Attachments.FirstOrDefault(x => x.Id == e.AttachmentId);
                
                if(attachment is null)
                    return;
                
                jobCard.Attachments.Remove(attachment);
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.JobCard, e.CardId));

                updated = true;
            }
            
            else if (_redCards.TryGetValue(e.CardId, out RedCardObject? redCard))
            {
                AttachmentInfo? attachment = redCard.Attachments.FirstOrDefault(x => x.Id == e.AttachmentId);
                
                if(attachment is null)
                    return;
                
                redCard.Attachments.Remove(attachment);
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.RedCard, e.CardId));

                updated = true;
            }
            
            else if (_yellowCards.TryGetValue(e.CardId, out RedCardObject? yellowCard))
            {
                AttachmentInfo? attachment = yellowCard.Attachments.FirstOrDefault(x => x.Id == e.AttachmentId);
                
                if(attachment is null)
                    return;
                
                yellowCard.Attachments.Remove(attachment);
                
                BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.YellowCard, e.CardId));
            }
            
            if(updated)
                Log.Logger.Debug("Attachment:{id} has been removed from board:{boardId}", e.AttachmentId, e.BoardId);
        }
    }

    public async void UpdateCCDashboardInfo(object? sender, CardUpdatedData e)
    {
        if (TrelloUtil.TryGetVanName(e.CardName, out _, out string name))
        {
            if (ProductionVans.TryGetValue(name, out VanProductionInfo? info))
            {
                GetCardOptions getCardOptions = new GetCardOptions()
                {
                    ActionsTypes = new ActionTypesToInclude("updateCard")
                };
                
                Card? trelloCard = await _trelloClient.GetCardAsync(e.CardId, getCardOptions);

                if (trelloCard is not null)
                {
                    info.HandoverHistory = [];
                    
                    var actions = trelloCard.Actions.Where(x => x.Data.Card.Due.HasValue);

                    foreach (TrelloAction action in actions.OrderBy(x => x.Date))
                    {
                        info.HandoverHistory.Add((action.Date, action.Data.Card.Due!.Value));
                    }
                    
                    info.HandoverState = !trelloCard.Due.HasValue
                                             ? HandoverState.Unknown
                                             : (trelloCard.DueComplete
                                                    ? HandoverState.HandedOver
                                                    : HandoverState.UnhandedOver);
                    
                    Log.Logger.Debug("Hanover information updated for {cardName}", e.CardName);
                    
                    if (!string.IsNullOrEmpty(info.Id) && !string.IsNullOrWhiteSpace(info.Id))
                    {
                        if (_vanBoards.TryGetValue(info.Id, out VanBoardObject? vanBoardObject))
                        {
                            vanBoardObject.Handover = trelloCard.Due;
                            Log.Logger.Debug("Van board {name}-{id} handover data updated", vanBoardObject.Name, vanBoardObject.Id);
                        }
                    }
                    
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.Hanover));
                }
            }
        }
    }

    public async void UpdatedLineMoveBoardInfo(object? sender, CardUpdatedData e)
    {
        if (TrelloUtil.TryGetVanName(e.CardName, out VanModel? type, out string name))
        {
            Card? trelloCard
                = await _trelloClient.GetCardAsync(
                      e.CardId, new GetCardOptions() { ActionsTypes = new ActionTypesToInclude(["updateCard:idList"]) });
            
            List<List> lists = await _trelloClient.GetListsOnBoardAsync(e.BoardId);
            
            List<(DateTimeOffset date, IProductionPosition)> positionHistory = trelloCard.Actions.ToPositionHistory(lists);
            
            if (ProductionVans.TryGetValue(name, out VanProductionInfo? info))
            {
                if (!info.PositionHistory.SequenceEqual(positionHistory))
                {
                    info.PositionHistory = positionHistory;
                
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(e.BoardId, BoardUpdateType.Position));
                }
            }
            else
            {
                var search = await TrySearchForVanId(name);
                
                if(search.boardfound && search.vanId is not null)
                {
                    VanProductionInfo newInfo = new VanProductionInfo(search.vanId.VanId, name, search.vanId.Url, positionHistory);
                    
                    if(ProductionVans.TryAdd(name, newInfo))
                        Log.Logger.Information("New van added to production {vanName}:{id}", newInfo.Name, newInfo.Id);
                }
            }
        }
    }
}
