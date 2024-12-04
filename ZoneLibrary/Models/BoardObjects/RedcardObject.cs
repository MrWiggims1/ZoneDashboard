using DBLibrary.Models;
using TrelloDotNet.Model;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class RedCardObject : IFilterableCard, IEqualityComparer<RedCardObject>, IEquatable<RedCardObject>
    {
        public string Id { get; }
        public string BoardId { get; }
        public string Name { get; private set; }
        public DateTimeOffset? CreationDate { get; }
        public string? CreatorMemberId { get; }
        public List<string> MemberIds { get; }
        public List<string> CommentIds { get; set; }
        public List<AttachmentInfo> Attachments { get; }
        public CardAreaOfOrigin AreaOfOrigin { get; private set; }
        public CardStatus CardStatus { get; private set; }
        public DateTimeOffset? CardStatusLastUpdated { get; private set; }
        public RedFlagIssue RedFlagIssue { get; private set; }

        public override string ToString() => $"RedCard {Id}: {Name}";

        internal RedCardObject(Card trelloCard, IEnumerable<CustomField> customFields, IEnumerable<CachedTrelloAction> cardActions, IEnumerable<string> commentIds, string? creatorMemberId)
        {
            this.Id = trelloCard.Id;
            this.Name = trelloCard.Name;
            this.BoardId = trelloCard.BoardId;
            this.CreationDate = trelloCard.Created;
            this.MemberIds = trelloCard.MemberIds;
            this.CreatorMemberId = creatorMemberId;

            IEnumerable<CustomField> enumerable = customFields.ToList();

            this.AreaOfOrigin = TrelloUtil.ToCardAreaOfOrigin(trelloCard, enumerable);
            this.RedFlagIssue = TrelloUtil.ToRedFlagIssue(trelloCard, enumerable);
            this.CommentIds = commentIds.ToList();

            this.CardStatus = TrelloUtil.ToCardStatus(trelloCard, enumerable, cardActions, out DateTimeOffset? cardStatusLastUpdated);
            this.CardStatusLastUpdated = cardStatusLastUpdated;

            if (trelloCard.Attachments is not null && trelloCard.Attachments.Count != 0)
            {
                var attachments = trelloCard.Attachments.Where(x => x.MimeType.StartsWith("image/"));
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
            this.RedFlagIssue = TrelloUtil.ToRedFlagIssue(trelloCard, enumerable);
            this.CardStatus = TrelloUtil.ToCardStatus(trelloCard, enumerable, actions, out DateTimeOffset? cardStatusLastUpdated);
            this.CardStatusLastUpdated = cardStatusLastUpdated;
        }

        public bool Equals(RedCardObject? other) => Equals(this, other);
        
        public bool Equals(RedCardObject? x, RedCardObject? y)
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
                   x.Name == y.Name && 
                   Nullable.Equals(x.CreationDate, y.CreationDate) && 
                   x.CreatorMemberId == y.CreatorMemberId && 
                   x.MemberIds.Equals(y.MemberIds) && 
                   x.CommentIds.Equals(y.CommentIds) && 
                   x.Attachments.Equals(y.Attachments) && 
                   x.AreaOfOrigin == y.AreaOfOrigin && 
                   x.CardStatus == y.CardStatus && 
                   Nullable.Equals(x.CardStatusLastUpdated, y.CardStatusLastUpdated) && 
                   x.RedFlagIssue == y.RedFlagIssue;
        }
        
        public CompareReport? Compare(RedCardObject? other)
        {
            CompareReport report = new CompareReport(ToString());
            
            if (ReferenceEquals(this, other)) 
                return null;
            
            if (ReferenceEquals(this, null)) 
            {
                report.Issues[ToString()].Add("This null Reference");
                report.Pass = false;
            }
            
            if (ReferenceEquals(other, null)) 
            {
                report.Issues[ToString()].Add("Other null Reference");
                report.Pass = false;

                return report;
            } 
            
            if(this.Id != other.Id)
            {
                report.Issues[ToString()].Add($"Id: {this.Id} != {other.Id}");
                report.Pass = false;
            }
            
            if(this.BoardId != other.BoardId)
            {
                report.Issues[ToString()].Add($"BoardId: {this.BoardId} != {other.BoardId}");
                report.Pass = false;
            }
            
            if(this.Name != other.Name)
            {
                report.Issues[ToString()].Add($"Name: {this.Name} != {other.Name}");
                report.Pass = false;
            }
            
            if(!Nullable.Equals(this.CreationDate,other.CreationDate))
            {
                report.Issues[ToString()].Add($"CreationDate: {this.CreationDate} != {other.CreationDate}");
                report.Pass = false;
            }
            
            if(this.CreatorMemberId != other.CreatorMemberId)
            {
                report.Issues[ToString()].Add($"CreatorMemberId: {this.CreatorMemberId} != {other.CreatorMemberId}");
                report.Pass = false;
            }
            
            if(this.AreaOfOrigin != other.AreaOfOrigin)
            {
                report.Issues[ToString()].Add($"AreaOfOrigin: {this.AreaOfOrigin} != {other.AreaOfOrigin}");
                report.Pass = false;
            }
            
            if(this.CardStatus != other.CardStatus)
            {
                report.Issues[ToString()].Add($"CardStatus: {this.CardStatus} != {other.CardStatus}");
                report.Pass = false;
            }
            
            if(!Nullable.Equals(this.CardStatusLastUpdated, other.CardStatusLastUpdated))
            {
                report.Issues[ToString()].Add($"CardStatusLastUpdated: {this.CardStatusLastUpdated} != {other.CardStatusLastUpdated}");
                report.Pass = false;
            }
            
            if(this.RedFlagIssue != other.RedFlagIssue)
            {
                report.Issues[ToString()].Add($"RedFlagIssue: {this.RedFlagIssue} != {other.RedFlagIssue}");
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
            
            List<string> otherMemberIds = new List<string>(other.MemberIds);
            
            foreach (string memberId in this.MemberIds)
            {
                if (otherMemberIds.Contains(memberId))
                {
                    otherMemberIds.Remove(memberId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Member Id {memberId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherMemberIds)
            {
                report.Issues[ToString()].Add($"Member Id {missedId} missing from this");
                report.Pass = false;
            }

            if (report.Pass == false)
                return report;

            else
                return null;
        }

        public int GetHashCode(RedCardObject obj)
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.BoardId);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.CreationDate);
            hashCode.Add(obj.CreatorMemberId);
            hashCode.Add(obj.MemberIds);
            hashCode.Add(obj.CommentIds);
            hashCode.Add(obj.Attachments);
            hashCode.Add((int)obj.AreaOfOrigin);
            hashCode.Add((int)obj.CardStatus);
            hashCode.Add(obj.CardStatusLastUpdated);
            hashCode.Add((int)obj.RedFlagIssue);
            return hashCode.ToHashCode();
        }
    }
}
