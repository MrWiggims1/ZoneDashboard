using DBLibrary.Models;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class CommentObject(CachedTrelloAction action) : IEqualityComparer<CommentObject>
    {
        public string Id { get; } = action.ActionId;
        public string CardId { get; } = action.CardId;
        public string BoardId { get; } = action.BoardId;
        public string CreatorMemberId { get; } = action.MemberId;
        public DateTimeOffset DateCreated { get; } = action.DateOffset;
        public string Content { get; } = string.IsNullOrEmpty(action.Content) ? "" : action.Content;

        public override string ToString() => $"Comment {Id}: {Content}";

        public bool Equals(CommentObject? other)
        {
            return Equals(this, other);
        }
        
        public bool Equals(CommentObject? x, CommentObject? y)
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
                   x.CardId == y.CardId && 
                   x.CreatorMemberId == y.CreatorMemberId && 
                   x.DateCreated.Equals(y.DateCreated) && 
                   x.Content == y.Content;
        }

        public CompareReport? Compare(CommentObject? other)
        {
            CompareReport report = new CompareReport(ToString());
            
            if (ReferenceEquals(this, other))
            {
                return null;
            }

            if (ReferenceEquals(this, null))
            {
                report.Issues[ToString()].Add("This null Reference");
                report.Pass = false;
                
                return report;
            }

            if (ReferenceEquals(other, null)) 
            {
                report.Issues[ToString()].Add("other null Reference");
                report.Pass = false;
                
                return report;
            }
            
            if(this.Id != other.Id )
            {
                 report.Issues[ToString()].Add($"Id: {this.Id} != {other.Id}");
                 report.Pass = false;
            }
            
            if(this.CardId != other.CardId )
            {
                 report.Issues[ToString()].Add($"CardId: {this.CardId} != {other.CardId}");
                 report.Pass = false;
            }
            
            if(this.CreatorMemberId != other.CreatorMemberId )
            {
                 report.Issues[ToString()].Add($"CreatorMemberId: {this.CreatorMemberId} != {other.CreatorMemberId}");
                 report.Pass = false;
            }
            
            if(!this.DateCreated.Equals(other.DateCreated))
            {
                 report.Issues[ToString()].Add($"DateCreated: {this.DateCreated} != {other.DateCreated}");
                 report.Pass = false;
            }
            
            if(this.Content != other.Content)
            {
                 report.Issues[ToString()].Add($"Content: {this.Content} != {other.Content}");
                 report.Pass = false;
            }

            if (report.Pass == false)
                return report;

            else
                return null;
        }

        public int GetHashCode(CommentObject obj)
        {
            return HashCode.Combine(obj.Id);
        }
    }
}
