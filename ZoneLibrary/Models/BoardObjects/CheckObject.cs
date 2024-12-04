using System.Drawing;
using TrelloDotNet.Model;
using ZoneProductionLibrary.Models.UpdateData;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class CheckObject : IEqualityComparer<CheckObject>, IEquatable<CheckObject>
    {
        public string Id { get; }
        public string BoardId { get; }
        public string CheckListId { get; private set; }
        public string Name { get; private set; }
        public bool IsChecked { get; private set; }
        public DateTimeOffset? LastModified { get; private set; }

        public override string ToString() => $"Check {Id}: {Name}";

        public CheckObject(string boardId, ChecklistItem checkItem, DateTimeOffset? lastModified)
        {
            this.BoardId = boardId;
            this.CheckListId = checkItem.ChecklistId;
            this.Id = checkItem.Id;
            this.Name = checkItem.Name;
            this.IsChecked = checkItem.State == ChecklistItemState.Complete;
            this.LastModified = lastModified;
        }

        internal CheckObject(CheckUpdatedData data)
        {
            this.BoardId = data.BoardId;
            this.CheckListId = data.CheckListId;
            this.Id = data.CheckId;
            this.Name = data.CheckName;
            this.IsChecked = data.IsChecked;
            this.LastModified = data.DateUpdated;
        }

        internal void UpdateName(string name) => this.Name = name;
        internal void UpdateCheckListId(string checkListId) => this.CheckListId = checkListId;
        internal void UpdateStatus(bool isChecked, DateTimeOffset dateUpdated)
        {
            this.LastModified = dateUpdated;
            this.IsChecked = isChecked;
        }

        public bool Equals(CheckObject? other)
        {
            return Equals(this, other);
        }
        
        public bool Equals(CheckObject? x, CheckObject? y)
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
                   x.CheckListId == y.CheckListId && 
                   x.Name == y.Name && 
                   x.IsChecked == y.IsChecked;
        }

        public CompareReport? Compare(CheckObject? other)
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
            
            if(this.BoardId != other.BoardId )
            {
                report.Issues[ToString()].Add($"BoardId: {this.BoardId} != {other.BoardId}");
                report.Pass = false;
            }
            
            if(this.CheckListId != other.CheckListId )
            {
                report.Issues[ToString()].Add($"CheckListId: {this.CheckListId} != {other.CheckListId}");
                report.Pass = false;
            }
            
            if(this.Name != other.Name )
            {
                report.Issues[ToString()].Add($"Name: {this.Name} != {other.Name}");
                report.Pass = false;
            }
            
            if(this.IsChecked != other.IsChecked )
            {
                report.Issues[ToString()].Add($"IsChecked: {this.IsChecked} != {other.IsChecked}");
                report.Pass = false;
            }
            
            if (report.Pass == false)
                return report;

            else
                return null;
        }

        public int GetHashCode(CheckObject obj)
        {
            return HashCode.Combine(obj.Id, obj.BoardId);
        }
    }
}
