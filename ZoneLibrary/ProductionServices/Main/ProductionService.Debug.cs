using Microsoft.Graph;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TrelloDotNet.Model.Webhook;
using File = System.IO.File;

namespace ZoneProductionLibrary.ProductionServices.Main
{
    public partial class ProductionService
    {
        public VanBoardObject? GetBoardObject(string boardId)
            => _vanBoards.Values.SingleOrDefault(x => x.Id == boardId);

        public IEnumerable<CheckObject> GetBoardCheckObjects(string boardId)
            => _checks.Values.Where(x => x.BoardId == boardId);

        public IEnumerable<ChecklistObject> GetBoardCheckListObjects(string boardId)
            => _checkLists.Values.Where(x => x.BoardId == boardId);

        public IEnumerable<JobCardObject> GetBoardJobCardObjects(string boardId)
            => _jobCards.Values.Where(x => x.BoardId == boardId);

        public IEnumerable<RedCardObject> GetBoardRedCardObjects(string boardId)
            => _redCards.Values.Where(x => x.BoardId == boardId);

        public async Task RemoveAllWebhooks()
        {
            foreach (string url in _webhooks.Select(x => x.CallbackUrl).Distinct())
            {
                await _trelloClient.DeleteWebhooksByCallbackUrlAsync(url);
            }

            _webhooks = new List<Webhook>();

            await CheckRequiredWebhooksActive();
        }

        public async Task RemoveWebhook(string idOfTypeYouWishToMonitor)
        {
            Webhook? webook = _webhooks.SingleOrDefault(x => x.IdOfTypeYouWishToMonitor == idOfTypeYouWishToMonitor);

            if (webook is not null)
            {
                await _trelloClient.DeleteWebhookAsync(webook.Id);
                _webhooks.Remove(webook);
            }
        }

        public async Task<CompareReport?> ForceReloadBoard(string id, bool saveReport = true)
        {
            if (TryRemoveVan(id,
                             false,
                             out VanBoardObject? oldBoard, 
                             out List<JobCardObject> jobCards, 
                             out List<ChecklistObject> checklists, 
                             out List<CheckObject> checks, 
                             out List<RedCardObject> redCards, 
                             out List<RedCardObject> yellowCards, 
                             out List<CommentObject> comments ))
            {
                
                Dictionary<string, JobCardObject> oldJobs = new Dictionary<string, JobCardObject>(
                    jobCards.Select(x => new KeyValuePair<string,JobCardObject>(x.Id, x))
                    );
                
                Dictionary<string, RedCardObject> oldRedCards = new Dictionary<string, RedCardObject>(
                    redCards.Select(x => new KeyValuePair<string,RedCardObject>(x.Id, x))
                    );
                
                Dictionary<string, RedCardObject> yellowRedCards = new Dictionary<string, RedCardObject>(
                    yellowCards.Select(x => new KeyValuePair<string,RedCardObject>(x.Id, x))
                    );
                
                Dictionary<string, CheckObject> oldChecks = new Dictionary<string, CheckObject>(
                    checks.Select(x => new KeyValuePair<string,CheckObject>(x.Id, x))
                    );
                
                Dictionary<string, ChecklistObject> oldCheckLists = new Dictionary<string, ChecklistObject>(
                    checklists.Select(x => new KeyValuePair<string,ChecklistObject>(x.Id, x))
                    );
                
                Dictionary<string, CommentObject> oldComments = new Dictionary<string, CommentObject>(
                    comments.Select(x => new KeyValuePair<string,CommentObject>(x.Id, x))
                    );
                
                // Collecting and comparing new data
                CompareReport? report = new CompareReport(oldBoard.Name);

                await GetBoardByIdAsync(id);

                if (_vanBoards.TryGetValue(id, out VanBoardObject? newBoard))
                {
                    report.Concat(newBoard.Compare(oldBoard));

                    foreach (string jobCardId in newBoard.JobCardIds)
                    {
                        if (_jobCards.TryGetValue(jobCardId, out JobCardObject? jobCard))
                        {
                            if (!oldJobs.Remove(jobCardId, out JobCardObject? oldJobCard))
                            {
                                report.Pass = false;
                                report.Issues[oldBoard.Name].Add($"{jobCard.Name}:{jobCard.Id} job card could not be found in old board.");
                                continue;
                            }

                            report.Concat(jobCard.Compare(oldJobCard));

                            foreach (string checkListId in jobCard.ChecklistIds)
                            {
                                if (_checkLists.TryGetValue(checkListId, out ChecklistObject? checkList))
                                {
                                    if (!oldCheckLists.Remove(checkListId, out ChecklistObject? oldCheckList))
                                    {
                                        report.Pass = false;
                                        report.Issues[oldBoard.Name].Add($"{checkList.Name}:{jobCard.Name} check list could not be found in old board.");
                                        continue;
                                    }
                                    
                                    report.Concat(checkList.Compare(oldCheckList));

                                    foreach (string checkId in checkList.CheckObjectIds)
                                    {
                                        if (_checks.TryGetValue(checkId, out CheckObject? check))
                                        {
                                            if (!oldChecks.Remove(checkId, out CheckObject? oldCheck))
                                            {
                                                report.Pass = false;
                                                report.Issues[oldBoard.Name].Add($"{check.Name}:{checkList.Name}:{jobCard.Name} check could not be found in old board.");
                                                continue;
                                            }
                                            
                                            report.Concat(check.Compare(oldCheck));
                                        }
                                    }
                                }
                            }

                            foreach (string commentId in jobCard.CommentIds)
                            {
                                if (_comments.TryGetValue(commentId, out CommentObject? comment))
                                {
                                    if (!oldComments.Remove(commentId, out CommentObject? oldComment))
                                    {
                                        report.Pass = false;
                                        report.Issues[oldBoard.Name].Add($"{comment.Content}:{jobCard.Name} comment could not be found in old board.");
                                        continue;
                                    }
                                    
                                    report.Concat(comment.Compare(oldComment));
                                }
                            }
                        }
                    }

                    foreach (string redCardId in oldBoard.RedCardIds)
                    {
                        if (_redCards.TryGetValue(redCardId, out RedCardObject? redCard))
                        {
                            if (!oldRedCards.Remove(redCardId, out RedCardObject? oldRedCard))
                            {
                                report.Pass = false;
                                report.Issues[oldBoard.Name].Add($"{redCard.Name} red card could not be found in old board.");
                                continue;
                            }
                            
                            report.Concat(oldRedCard.Compare(redCard));

                            foreach (string commentId in redCard.CommentIds)
                            {
                                if (_comments.TryGetValue(commentId, out CommentObject? comment))
                                {
                                    if (!oldComments.Remove(commentId, out CommentObject? oldComment))
                                    {
                                        report.Pass = false;
                                        report.Issues[oldBoard.Name].Add($"{comment.Content}:{redCard.Name} comment could not be found in old board.");
                                        continue;
                                    }
                                    
                                    report.Concat(comment.Compare(oldComment));
                                }
                            }
                        }
                    }
                }

                foreach (CheckObject check in oldChecks.Values)
                {
                    report.Pass = false;
                    report.Issues[oldBoard.Name].Add($"Check: {check.Name} should have been removed.");
                }
                
                foreach (ChecklistObject checkList in oldCheckLists.Values)
                {
                    report.Pass = false;
                    report.Issues[oldBoard.Name].Add($"Checklist: {checkList.Name} should have been removed.");
                }
                
                foreach (JobCardObject job in oldJobs.Values)
                {
                    report.Pass = false;
                    report.Issues[oldBoard.Name].Add($"JobCard: {job.Name} should have been removed.");
                }
                
                foreach (RedCardObject redCard in oldRedCards.Values)
                {
                    report.Pass = false;
                    report.Issues[oldBoard.Name].Add($"RedCard: {redCard.Name} should have been removed.");
                }
                
                foreach (CommentObject comment in oldComments.Values)
                {
                    report.Pass = false;
                    report.Issues[oldBoard.Name].Add($"Comment: {comment.Content} should have been removed.");
                }

                if (!report.Pass && saveReport)
                {
                    string filePath = $"Logs/CompareReports/{oldBoard.Name}-{DateTime.Now.ToString("dd-MM-yyyy")}.json";
                    string content = JsonConvert.SerializeObject(report, Formatting.Indented);
                    
                    if(File.Exists(filePath))
                        File.Delete(filePath);

                    await using (FileStream fs = File.Create(filePath))
                    {
                        byte[] text = new UTF8Encoding(true).GetBytes(content);
                        await fs.WriteAsync(text);
                    }
                    
                    Log.Warning("Board reload failed test:\n{results}", content);
                    
                    BoardUpdated?.Invoke(this, new BoardUpdateInfo(id));
                    
                    return report;
                }

                else
                    return null;
            }
            else
            {
                throw new KeyNotFoundException($"Could not find van with Id: {id}");
            }
        }

        public async Task ReloadProductionInfo()
        {
            
        }

        public bool CheckBoardIsUnloaded(string id)
        {
            bool pass = true;
            
            string vanIdent = id;

            if (ProductionVans.TryGetValue(id, out VanProductionInfo? info))
                vanIdent = info.Name;
            
            if (_vanBoards.TryGetValue(id, out VanBoardObject? van))
            {
                pass = false;
                Log.Logger.Error("{van} van board object is loaded.", van.Name);
            }

            var jobCards = _jobCards.Values.Where(x => x.BoardId == id);
            var redCards = _redCards.Values.Where(x => x.BoardId == id);
            var yellowCards = _yellowCards.Values.Where(x => x.BoardId == id);
            var comments = _comments.Values.Where(x => x.BoardId == id);
            var checklists = _checkLists.Values.Where(x => x.BoardId == id);
            var checks = _checks.Values.Where(x => x.BoardId == id);

            foreach (JobCardObject jobCard in jobCards)
            {
                pass = false;
                Log.Logger.Error("{jobCard} job card object is loaded on board {van}", jobCard.Name, vanIdent);
            }

            foreach (RedCardObject redCard in redCards)
            {
                pass = false;
                Log.Logger.Error("{redCard} red card object is loaded on board {van}", redCard.Name, vanIdent);
            }

            foreach (RedCardObject yellowCard in yellowCards)
            {
                pass = false;
                Log.Logger.Error("{yellowCard} yellow card object is loaded on board {van}", yellowCard.Name, vanIdent);
            }

            foreach (ChecklistObject checklist in checklists)
            {
                pass = false;
                Log.Logger.Error("{checkList} check list object is loaded on board {van}", checklist.Name, vanIdent);
            }

            foreach (CheckObject check in checks)
            {
                pass = false;
                Log.Logger.Error("{check} check object is loaded on board {van}", check.Name, vanIdent);
            }

            foreach (CommentObject comment in comments)
            {
                pass = false;
                Log.Logger.Error("{comment} comment object is loaded on board {van}", comment.Id, vanIdent);
            }

            return pass;
        }
    }
}