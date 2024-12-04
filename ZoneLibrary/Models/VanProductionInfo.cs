namespace ZoneProductionLibrary.Models
{
    public class VanProductionInfo : IFilterableBoard, IEqualityComparer<VanProductionInfo>, IEquatable<VanProductionInfo>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public VanModel VanModel => Name.ToVanType();
        public IProductionPosition Position => PositionHistory.Count > 0 ? PositionHistory.Last().position : new PreProduction();
        public List<(DateTimeOffset date, IProductionPosition position)> PositionHistory { get; set; }
        
        public List<(DateTimeOffset changeDate, DateTimeOffset HandoverDate)> HandoverHistory { get; set; } = [];

        public DateTimeOffset? Handover => HandoverHistory.Any() ? HandoverHistory.Last().HandoverDate : null;
        public HandoverState HandoverState { get; set; } = HandoverState.Unknown;

        public TimeSpan? TimeToHandover => Handover.HasValue ? Handover.Value - DateTimeOffset.Now : null;
        public bool IsInCarPark => Position.IsInCarPark(HandoverState);

        public override string ToString() => $"{Name}:{Id}";

        public VanProductionInfo(string id, string name, string url, List<(DateTimeOffset, IProductionPosition)> positionHistory)
        {
            Id = id;
            Name = name;
            Url = url;
            PositionHistory = positionHistory.ToList();
        }

        public VanProductionInfo(string id, string name, string url, List<(DateTimeOffset, IProductionPosition)> positionHistory, List<(DateTimeOffset, DateTimeOffset)> handoverHistory, HandoverState handoverState)
        {
            Id = id;
            Name = name;
            Url = url;
            PositionHistory = positionHistory.ToList();
            HandoverHistory = handoverHistory.ToList();
            HandoverState = handoverState;
        }
        
        
        /// <param name="timeSpan">How new the change has to be</param>
        /// <param name="date">How close the handover date has to be</param>
        public bool HasHandoverDateChanged(TimeSpan timeSpan, TimeSpan date, bool ignoreDueVans = true)
        {
            if (HandoverHistory.Count < 2)
                return false;

            DateTimeOffset lastDate = HandoverHistory.TakeLast(2).First().HandoverDate;
            DateTimeOffset newDate = HandoverHistory.Last().HandoverDate;

            if (ignoreDueVans && HandoverState == HandoverState.HandedOver && newDate < DateTimeOffset.Now)
                return false;

            if (lastDate.LocalDateTime.Date == newDate.LocalDateTime.Date)
                return false;

            if (newDate - date > DateTimeOffset.Now || lastDate - date > DateTimeOffset.Now)
                return false;

            return (HandoverHistory.Last().changeDate + timeSpan > DateTimeOffset.Now );
        }

        public bool Equals(VanProductionInfo? other) => Equals(this, other);

        
        public bool Equals(VanProductionInfo? x, VanProductionInfo? y)
        {
            if (ReferenceEquals(x, y)) 
                return true;
            
            if (ReferenceEquals(x, null)) 
                return false;
            
            if (ReferenceEquals(y, null)) 
                return false;
            
            if (x.GetType() != y.GetType()) 
                return false;
            
            return x.Id == y.Id && x.Name == y.Name && x.PositionHistory.Equals(y.PositionHistory) && x.HandoverHistory.Equals(y.HandoverHistory) && x.HandoverState == y.HandoverState;
        }

        public int GetHashCode(VanProductionInfo obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, obj.PositionHistory, obj.HandoverHistory, (int)obj.HandoverState);
        }

        public CompareReport? Compare(VanProductionInfo? other)
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

            if (this.Id != other.Id)
            {
                report.Issues[ToString()].Add($"Id: {this.Id} != {other.Id}");
                report.Pass = false;
            }
            if (this.Name != other.Name)
            {
                report.Issues[ToString()].Add($"Name: {this.Name} != {other.Name}");
                report.Pass = false;
            }
            if (this.HandoverState != other.HandoverState)
            {
                report.Issues[ToString()].Add($"Hand over state: {this.Name} != {other.Name}");
                report.Pass = false;
            }

            foreach ((DateTimeOffset changeDate, DateTimeOffset HandoverDate) handoverChange in HandoverHistory)
            {
                if (!other.HandoverHistory.Contains(handoverChange))
                {
                    report.Issues[ToString()].Add($"Handover change {handoverChange} missing from other");
                    report.Pass = false;
                }
            }

            foreach ((DateTimeOffset changeDate, DateTimeOffset HandoverDate) handoverChange in other.HandoverHistory)
            {
                if (!HandoverHistory.Contains(handoverChange))
                {
                    report.Issues[ToString()].Add($"Handover change {handoverChange} missing from this");
                    report.Pass = false;
                }
            }

            foreach ((DateTimeOffset date, IProductionPosition position) positionChange in PositionHistory)
            {
                if (!other.PositionHistory.Contains(positionChange))
                {
                    report.Issues[ToString()].Add($"Position change {positionChange} missing from other");
                    report.Pass = false;
                }
            }
            
            foreach ((DateTimeOffset date, IProductionPosition position) positionChange in other.PositionHistory)
            {
                if (!PositionHistory.Contains(positionChange))
                {
                    report.Issues[ToString()].Add($"Position change {positionChange} missing from this");
                    report.Pass = false;
                }
            }
            
            if (report.Pass == false)
                return report;

            else
                return null;
        }
    }

    public enum HandoverState
    {
        Unknown,
        UnhandedOver,
        HandedOver
    }
}
