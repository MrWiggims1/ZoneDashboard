using DBLibrary.Models;
using TrelloDotNet.Model;
using ZoneProductionLibrary.Models.UpdateData;
using ZoneProductionLibrary.ProductionServices.Main;
using Checklist = TrelloDotNet.Model.Checklist;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class ChecklistObject : IEqualityComparer<ChecklistObject>, IEquatable<ChecklistObject>
    {
        public string Id { get; }
        public string BoardId { get;}
        public string Name { get; private set; }
        public string CardId { get; }
        public List<string> CheckObjectIds { get; } = [];

        public override string ToString() => $"Checklist {Id}: {Name}";

        internal ChecklistObject(ProductionService productionService, string boardId, Checklist checkList, IEnumerable<CachedTrelloAction> checkStateActions)
        {
            this.BoardId = boardId;
            this.CardId = checkList.CardId;
            this.Id = checkList.Id;
            this.Name = checkList.Name;

            IEnumerable<CachedTrelloAction> trelloActions = checkStateActions.ToList();

            foreach (ChecklistItem? check in checkList.Items)
            {
                List<CachedTrelloAction> actions = trelloActions.Where(x => x.CheckId == check.Id).ToList();
                DateTimeOffset? lastModified = null;

                if (actions.Count != 0)
                {
                    lastModified = actions.FirstOrDefault()?.DateOffset;
                }
                
                productionService._checks.TryAdd(check.Id, new CheckObject(boardId, check, lastModified));
                this.CheckObjectIds.Add(check.Id);
            }
        }

        internal ChecklistObject(CheckListCreatedData data)
        {
            this.BoardId = data.BoardId;
            this.CardId = data.CardId;
            this.Id = data.CheckListId;
            this.Name = data.CheckListName;
        }

        internal void UpdateName(string name) => this.Name = name;

        internal void AddCheckId(string id) => this.CheckObjectIds.Add(id);

        internal void RemoveCheckId(string id) => this.CheckObjectIds.Remove(id);

        public bool Equals(ChecklistObject? other) => Equals(this, other);
        
        public bool Equals(ChecklistObject? x, ChecklistObject? y)
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
                   x.CardId == y.CardId && 
                   x.CheckObjectIds.Equals(y.CheckObjectIds);
        }
        
        public CompareReport? Compare(ChecklistObject? other)
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
            
            if(this.Name != other.Name )
            {
                report.Issues[ToString()].Add($"Name: {this.Name} != {other.Name}");
                report.Pass = false;
            }
            
            if(this.CardId != other.CardId )
            {
                report.Issues[ToString()].Add($"CardId: {this.CardId} != {other.CardId}");
                report.Pass = false;
            }

            List<string> otherCheckList = new List<string>(other.CheckObjectIds);
            
            foreach (string checkId in this.CheckObjectIds)
            {
                if (otherCheckList.Contains(checkId))
                {
                    otherCheckList.Remove(checkId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Check Id {checkId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherCheckList)
            {
                report.Issues[ToString()].Add($"Check Id {missedId} missing from this");
                report.Pass = false;
            }
            
            if (report.Pass == false)
                return report;

            else
                return null;
        }

        public int GetHashCode(ChecklistObject obj)
        {
            return HashCode.Combine(obj.Id, obj.BoardId);
        }
    }
}
