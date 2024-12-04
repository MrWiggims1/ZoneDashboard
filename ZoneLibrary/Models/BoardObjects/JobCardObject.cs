using DBLibrary.Models;
using TrelloDotNet.Model;
using ZoneProductionLibrary.ProductionServices.Main;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class JobCardObject : IFilterableCard, IEqualityComparer<JobCardObject>, IEquatable<JobCardObject>
    {
        public string Id { get; }
        public string BoardId { get; }
        public string TrelloListName { get; }
        public string Name { get; private set; }
        public string Url => $"https://trello.com/c/{this.Id}/";
        public CardAreaOfOrigin AreaOfOrigin { get; private set; }
        public CardStatus CardStatus { get; private set; }
        public DateTimeOffset? CardStatusLastUpdated { get; private set; }
        public TimeSpan TaskTime { get; private set; }
        public List<string> ChecklistIds { get; } = new List<string>();
        public List<string> CommentIds { get; set; }
        public List<AttachmentInfo> Attachments { get; }

        public override string ToString() => $"JobCard {Id}: {Name}";

        internal JobCardObject(
            ProductionService productionService, 
            Card trelloCard, 
            IEnumerable<CustomField> customFields, 
            IEnumerable<CachedTrelloAction> actions,  
            IEnumerable<string> commentIds)
        {
            IEnumerable<CachedTrelloAction> cachedTrelloActions = actions.ToList();
            IEnumerable<CustomField> enumerable = customFields.ToList();
            
            List<CachedTrelloAction>  cardFieldUpdates = cachedTrelloActions.Where(x => x.ActionType == "updateCustomFieldItem").ToList();
            List<CachedTrelloAction> checkActions = cachedTrelloActions.Where(x => x.ActionType == "updateCheckItemStateOnCard").ToList();
            
            this.Id = trelloCard.Id;
            this.Name = trelloCard.Name;
            this.BoardId = trelloCard.BoardId;
            this.TrelloListName = trelloCard.List.Name;

            this.AreaOfOrigin = TrelloUtil.ToCardAreaOfOrigin(trelloCard, enumerable);
            this.TaskTime = TrelloUtil.GetTaskTime(trelloCard, enumerable);
            
            this.CardStatus = TrelloUtil.ToCardStatus(trelloCard, enumerable, cardFieldUpdates, out DateTimeOffset? cardStatusLastUpdated);
            this.CardStatusLastUpdated = cardStatusLastUpdated;

            this.CommentIds = commentIds.ToList();

            foreach (var checklist in trelloCard.Checklists)
            {
                productionService._checkLists.TryAdd(checklist.Id, new ChecklistObject(productionService, trelloCard.BoardId, checklist, checkActions));
                this.ChecklistIds.Add(checklist.Id);
            }
            
            if (trelloCard.Attachments is not null && trelloCard.Attachments.Count != 0)
            {
                var attachments = trelloCard.Attachments.Where(x => !string.IsNullOrEmpty(x.MimeType) && !string.IsNullOrWhiteSpace(x.MimeType) && x.MimeType.StartsWith("image/"));
                this.Attachments = new List<AttachmentInfo>();

                foreach (Attachment attachment in attachments) 
                {
                    this.Attachments.Add(new AttachmentInfo(attachment));
                }
            }
            else
                this.Attachments = new List<AttachmentInfo>();
        }
        
        internal void UpdateCustomFieldsItems(Card trelloCard, IEnumerable<CustomField> customFields, IEnumerable<CachedTrelloAction> actions)
        {
            IEnumerable<CustomField> enumerable = customFields.ToList();

            this.AreaOfOrigin = TrelloUtil.ToCardAreaOfOrigin(trelloCard, enumerable);
            this.TaskTime = TrelloUtil.GetTaskTime(trelloCard, enumerable);

            this.CardStatus = TrelloUtil.ToCardStatus(trelloCard, enumerable, actions, out DateTimeOffset? cardStatusLastUpdated);
            this.CardStatusLastUpdated = cardStatusLastUpdated;
        }
        
        internal void RemoveChecklistId(string id) => this.ChecklistIds.Remove(id);
        internal void AddChecklistId(string id) => this.ChecklistIds.Add(id);
        
        public bool Equals(JobCardObject? other) => Equals(this, other);
        
        public bool Equals(JobCardObject? x, JobCardObject? y)
        {
            if (ReferenceEquals(x, y)) 
                return true;
            
            if (ReferenceEquals(x, null)) 
                return false;
            
            if (ReferenceEquals(y, null)) 
                return false;
            
            if (x.GetType() != y.GetType()) 
                return false;
            
            return x.Id == y.Id && 
                   x.BoardId == y.BoardId && 
                   x.TrelloListName == y.TrelloListName && 
                   x.Name == y.Name && 
                   x.AreaOfOrigin == y.AreaOfOrigin && 
                   x.CardStatus == y.CardStatus && 
                   Nullable.Equals(x.CardStatusLastUpdated, y.CardStatusLastUpdated) 
                   && x.TaskTime.Equals(y.TaskTime)
                   && x.ChecklistIds.Equals(y.ChecklistIds);
        }

        public CompareReport? Compare(JobCardObject? other)
        {
            CompareReport report = new CompareReport(ToString());

            if (ReferenceEquals(this, other))
                return null;

            if (ReferenceEquals(this, null))
            {
                report.Issues[ToString()].Add("This null Reference");
                report.Pass = false;

                return report;
            }

            if (ReferenceEquals(other, null))
            {
                report.Issues[ToString()].Add("Other null Reference");
                report.Pass = false;

                return report;
            }

            if (this.Id != other.Id)
            {
                report.Issues[ToString()].Add($"Id: {this.Id } != { other.Id}");
                report.Pass = false;
            }

            if (this.BoardId != other.BoardId)
            {
                report.Issues[ToString()].Add($"BoardId: {this.BoardId } != { other.BoardId}");
                report.Pass = false;
            }

            if (this.TrelloListName != other.TrelloListName)
            {
                report.Issues[ToString()].Add($"TrelloListName: {this.TrelloListName } != { other.TrelloListName}");
                report.Pass = false;
            }

            if (this.Name != other.Name)
            {
                report.Issues[ToString()].Add($"Name: {this.Name } != { other.Name}");
                report.Pass = false;
            }

            if (this.AreaOfOrigin != other.AreaOfOrigin)
            {
                report.Issues[ToString()].Add($"Area: {this.AreaOfOrigin } != { other.AreaOfOrigin}");
                report.Pass = false;
            }

            if (this.CardStatus != other.CardStatus)
            {
                report.Issues[ToString()].Add($"CardStatus: {this.CardStatus } != { other.CardStatus}");
                report.Pass = false;
            }

            if (!Nullable.Equals(this.CardStatusLastUpdated, other.CardStatusLastUpdated))
            {
                report.Issues[ToString()].Add($"StatusLastUpdated: {this.CardStatusLastUpdated} != {other.CardStatusLastUpdated}");
                report.Pass = false;
            }

            if (this.TaskTime != other.TaskTime)
            {
                report.Issues[ToString()].Add($"TaskTime: {this.TaskTime} != {other.TaskTime}");
                report.Pass = false;
            }
            
            List<string> otherCheckListIds = new List<string>(other.ChecklistIds);
            
            foreach (string checkListId in this.ChecklistIds)
            {
                if (otherCheckListIds.Contains(checkListId))
                {
                    otherCheckListIds.Remove(checkListId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Check list Id {checkListId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherCheckListIds)
            {
                report.Issues[ToString()].Add($"Check list Id {missedId} missing from this");
                report.Pass = false;
            }
            
            List<string> otherCommentIds = new List<string>(other.CommentIds);
            
            foreach (string commentId in this.CommentIds)
            {
                if (otherCommentIds.Contains(commentId))
                {
                    otherCommentIds.Remove(commentId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Comment Id {commentId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherCommentIds)
            {
                report.Issues[ToString()].Add($"Comment Id {missedId} missing from this");
                report.Pass = false;
            }
            
            List<string> otherAttachmentIds = new List<string>(other.Attachments.Select(x => x.Id));
            
            foreach (string attachmentId in this.Attachments.Select(x => x.Id))
            {
                if (otherAttachmentIds.Contains(attachmentId))
                {
                    otherAttachmentIds.Remove(attachmentId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Attachment Id {attachmentId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherAttachmentIds)
            {
                report.Issues[ToString()].Add($"Attachment Id {missedId} missing from this");
                report.Pass = false;
            }

            return report;
        }

        public int GetHashCode(JobCardObject obj)
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.BoardId);
            hashCode.Add(obj.TrelloListName);
            hashCode.Add(obj.Name);
            hashCode.Add((int)obj.AreaOfOrigin);
            hashCode.Add((int)obj.CardStatus);
            hashCode.Add(obj.CardStatusLastUpdated);
            hashCode.Add(obj.TaskTime);
            hashCode.Add(obj.ChecklistIds);
            return hashCode.ToHashCode();
        }
    }
}
