using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ZoneProductionLibrary.ProductionServices.Main
{
    public partial class ProductionService : IProductionService
    {
        public bool TryRemoveVanInfo(string id, bool removeWebhook, [NotNullWhen(true)] out VanProductionInfo? info)
        {
            TryRemoveVan(id, removeWebhook);

            if (this.ProductionVans.Remove(id, out info))
            {
                Log.Logger.Warning("{van} has been removed from production.", info.Name);
                return true;
            }

            return false;
        }

        public bool TryRemoveVan(string id, bool removeWebhook) => TryRemoveVan(id, removeWebhook, out _, out _, out _, out _, out _, out _, out _);
        public bool TryRemoveVan(string id,
                                 bool removeWebhook,
                                 [NotNullWhen(true)] out VanBoardObject? van, 
                                 out List<JobCardObject> jobCards, 
                                 out List<ChecklistObject> checklists,
                                 out List<CheckObject> checks,
                                 out List<RedCardObject> redCards,
                                 out List<RedCardObject> yellowCards,
                                 out List<CommentObject> comments)
        {
            VanProductionInfo? info = ProductionVans.Values.SingleOrDefault(x => x.Id == id);
            
            if (info is null)
            {
                Log.Logger.Error("Van with id {id} does not exist, could not remove.", id);
                throw new KeyNotFoundException();
            }
            
            jobCards = [];
            checklists = [];
            checks = [];
            redCards = [];
            yellowCards = [];
            comments = [];

            if (!_vanBoards.TryRemove(id, out van))
            {
                Log.Logger.Warning("Could not remove {van} from production service", info.Name);
                return false;
            }
            
            Log.Logger.Debug("Unloading {van}", info.Name);

            List<string> cardIds = [];
            
            cardIds.AddRange(van.JobCardIds);
            cardIds.AddRange(van.RedCardIds);
            cardIds.AddRange(van.YellowCardIds);
            
            foreach (string cardId in cardIds)
            {
                if (TryRemoveCard(cardId, out JobCardObject? jobCard, out RedCardObject? redCard, out RedCardObject? yellowCard, out List<ChecklistObject> jobChecklists, out List<CheckObject> jobChecks, out List<CommentObject> jobComments))
                {
                    if(jobCard is not null)
                        jobCards.Add(jobCard);
                    
                    if(redCard is not null)
                        redCards.Add(redCard);
                    
                    if(yellowCard is not null)
                        yellowCards.Add(yellowCard);
                    
                    checklists.AddRange(jobChecklists);
                    checks.AddRange(jobChecks);
                    comments.AddRange(jobComments);
                }
            }

            if(removeWebhook)
                Task.Run(async () => { RemoveWebhook(id); });
            
            Log.Logger.Information("{vanName} - H/O: {handOver}:{handoverState} - {pos} has been unloaded", 
                                   van.Name, 
                                   van.Handover.HasValue ? van.Handover.Value.LocalDateTime.ToString("dd/MM/yy hh tt") : "na", 
                                   this.ProductionVans[van.Name].HandoverState, 
                                   this.ProductionVans[van.Name].Position.PositionName);

            Debug.Assert(CheckBoardIsUnloaded(id));
            
            return true;
        }

        public bool TryRemoveCard(string id) => TryRemoveCard(id, out _, out _, out _, out _, out _, out _);
        public bool TryRemoveCard(string id, out CardType cardType)
        {
            cardType = CardType.None;
            
            bool result = TryRemoveCard(id,
                                        out JobCardObject? jobCard,
                                        out RedCardObject? redCard,
                                        out RedCardObject? yellowCard,
                                        out List<ChecklistObject> _,
                                        out List<CheckObject> _,
                                        out List<CommentObject> _);

            if (result)
            {
                if (jobCard is not null)
                    cardType = CardType.JobCard;

                if (redCard is not null)
                    cardType = CardType.RedCard;

                if (yellowCard is not null)
                    cardType = CardType.YellowCard;
            }

            return result;
        }
        
        public bool TryRemoveCard(string id,
                                  out JobCardObject? jobCard,
                                  out RedCardObject? redCard,
                                  out RedCardObject? yellowCard,
                                  out List<ChecklistObject> checklists,
                                  out List<CheckObject> checks,
                                  out List<CommentObject> comments)
        {
            jobCard = null;
            redCard = null;
            yellowCard = null;
            checklists = [];
            checks = [];
            comments = [];
            
            if (_jobCards.ContainsKey(id))
                return TryRemoveJobCard(id, out jobCard, out checklists, out checks, out comments);
            
            else if (_redCards.ContainsKey(id))
                return TryRemoveRedCard(id, out redCard, out comments);
            
            else if (_yellowCards.ContainsKey(id))
                return TryRemoveYellowCard(id, out yellowCard, out comments);

            return false;
        }
        
        public bool TryRemoveJobCard(string id) => TryRemoveJobCard(id, out _, out _, out _, out _);
        public bool TryRemoveJobCard(
            string id,
            [NotNullWhen(true)] out JobCardObject? jobCard,
            out List<ChecklistObject> checklists,
            out List<CheckObject> checks,
            out List<CommentObject> comments)
        {
            checklists = [];
            checks = [];
            comments = [];
            
            if (!_jobCards.TryRemove(id, out jobCard))
            {
                Log.Logger.Warning("Could not remove job card with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing job card {name}:{id}", jobCard.Name, id);

            foreach (string checklistId in jobCard.ChecklistIds)
            {
                if (TryRemoveCheckList(checklistId, out ChecklistObject? checklist, out List<CheckObject> checksInList))
                {
                    checklists.Add(checklist);
                    checks.AddRange(checksInList);
                }
            }

            foreach (string commentId in jobCard.CommentIds)
            {
                if(TryRemoveComment(commentId, out CommentObject? comment))
                    comments.Add(comment);
            }

            if (_vanBoards.TryGetValue(jobCard.BoardId, out VanBoardObject? van))
                van.JobCardIds.Remove(id);

            return true;
        }

        public bool TryRemoveRedCard(string id) => TryRemoveRedCard(id, out _, out _);
        public bool TryRemoveRedCard(string id, [NotNullWhen(true)] out RedCardObject? redCard, out List<CommentObject> comments)
        {
            comments = [];
            
            if (!_redCards.TryRemove(id, out redCard))
            {
                Log.Logger.Warning("Could not remove red card with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing red card {name}:{id}", redCard.Name, id);

            foreach (string commentId in redCard.CommentIds)
            {
                if(TryRemoveComment(commentId, out CommentObject? comment))
                    comments.Add(comment);
            }

            if (_vanBoards.TryGetValue(redCard.BoardId, out VanBoardObject? van))
                van.RedCardIds.Remove(id);

            return true;
        }
        
        public bool TryRemoveYellowCard(string id) => TryRemoveYellowCard(id, out _, out _);
        public bool TryRemoveYellowCard(string id, [NotNullWhen(true)] out RedCardObject? yellowCard, out List<CommentObject> comments)
        {
            comments = [];
            
            if (!_yellowCards.TryRemove(id, out yellowCard))
            {
                Log.Logger.Warning("Could not remove yellow card with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing yellow card {name}:{id}", yellowCard.Name, id);

            foreach (string commentId in yellowCard.CommentIds)
            {
                if(TryRemoveComment(commentId, out CommentObject? comment))
                    comments.Add(comment);
            }

            if (_vanBoards.TryGetValue(yellowCard.BoardId, out VanBoardObject? van))
                van.YellowCardIds.Remove(id);

            return true;
        }

        public bool TryRemoveCheckList(string id) => TryRemoveCheckList(id, out _, out _);
        public bool TryRemoveCheckList(string id, [NotNullWhen(true)] out ChecklistObject? checklist, out List<CheckObject> checks)
        {
            checks = [];

            if (!_checkLists.TryRemove(id, out checklist))
            {
                Log.Logger.Warning("Could not remove check list with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing check list {name}:{id}", checklist.Name, id);

            foreach (string checkObjectId in checklist.CheckObjectIds)
            {
                if(TryRemoveCheck(checkObjectId, out CheckObject? check))
                    checks.Add(check);
            }

            if (_jobCards.TryGetValue(id, out JobCardObject? jobCard))
                jobCard.ChecklistIds.Remove(id);

            return true;
        }

        public bool TryRemoveCheck(string id) => TryRemoveCheck(id, out _);
        public bool TryRemoveCheck(string id, [NotNullWhen(true)] out CheckObject? check)
        {
            if (!_checks.TryRemove(id, out check))
            {
                Log.Logger.Warning("Could not remove check with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing check {name}:{id}", check.Name, id);

            if (_checkLists.TryGetValue(check.CheckListId, out ChecklistObject? checklist))
                checklist.CheckObjectIds.Remove(id);

            return true;
        }

        public bool TryRemoveComment(string id) => TryRemoveComment(id, out _);
        public bool TryRemoveComment(string id, [NotNullWhen(true)] out CommentObject? comment)
        {
            if(!_comments.TryRemove(id, out comment))
            {
                Log.Logger.Warning("Could not remove comment with id {id}", id);
                return false;
            }
            
            Log.Logger.Debug("Removing comment {name}:{id}", comment.Content, id);

            if (_jobCards.TryGetValue(comment.CardId, out JobCardObject? job))
                job.CommentIds.Remove(id);

            else if (_redCards.TryGetValue(comment.CardId, out RedCardObject? redCard))
                redCard.CommentIds.Remove(id);

            if (_yellowCards.TryGetValue(comment.CardId, out RedCardObject? yellowCard))
                yellowCard.CommentIds.Remove(id);

            return true;
        }

        public async Task ProductionServiceCleanUp()
        {
            List<string> ids = (this as IProductionService).RequiredBoardIds().ToList();
            List<string> unloadedIds = ids.Where(x => !_vanBoards.ContainsKey(x)).ToList();
            int boardsRemoved = 0;

            foreach (string id in _vanBoards.Keys.Where(x => !ids.Contains(x)))
            {
                if (TryRemoveVan(id, true))
                    boardsRemoved++;
            }

            foreach (string unloadedId in unloadedIds)
            {
                await GetBoardByIdAsync(unloadedId);

                ids.Remove(unloadedId);
            }

            foreach (string id in ids)
            {
                await ForceReloadBoard(id);
            }
            
            Log.Logger.Information("Production service cleanup completed, removed {boardsRemoved}, loaded {loadCount} and reloaded {reloadCount} vans", boardsRemoved, unloadedIds.Count, ids.Count);
        }
    }
}