using Newtonsoft.Json.Converters;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text.Json.Serialization;
using ZoneProductionLibrary.ProductionServices.Main;

namespace ZoneProductionLibrary.Models.Boards
{
    public class JobCard : IFilterableCard
    {
        public string Id { get; }
        [JsonIgnore] public string BoardId { get; }
        [JsonIgnore] public string BoardName { get; }
        [JsonIgnore] public TypeOfVan VanType => BoardName.ToVanType().IsGen2() ? TypeOfVan.Gen2 : TypeOfVan.Expo;
        public string Name { get; }
        public string Url => $"https://trello.com/c/{Id}/";
        public CardStatus CardStatus
        {
            get
            {
                if (this.CompletionRate > .999)
                    return CardStatus.Completed;

                if (_cardStatus is CardStatus.UnableToComplete)
                    return _cardStatus;

                if (_cardStatus is CardStatus.InProgress || this.CompletionRate is > 0d and < 1d)
                    return CardStatus.InProgress;

                return _cardStatus;
            }
        }

        [JsonIgnore] private CardStatus _cardStatus;
        public DueStatus DueStatus { get; }
        [JsonIgnore] public DateTimeOffset? CardStatusLastUpdated { get; private set; } // TODO: account for status not being changed and all checks being marked off
        public CardAreaOfOrigin AreaOfOrigin { get; }
        [JsonIgnore] public string TrelloListName { get; }
        public IProductionPosition Position { get; }
        public List<Checklist> CheckLists { get; } = new List<Checklist>();
        [JsonIgnore] public DateTimeOffset? Handover { get; }
        [JsonIgnore] public TimeSpan TimeToHandover => Handover.HasValue ? Handover.Value - DateTimeOffset.Now : TimeSpan.MaxValue;
        [JsonIgnore] private TimeSpan _taskTime { get; }
        public TimeSpan TaskTime => _taskTime > TimeSpan.Zero ? _taskTime : TimeSpan.FromMinutes(30);
        [JsonIgnore] public TimeSpan RemainingTaskTime => TaskTime - (TaskTime * CompletionRate);
        public List<Comment> Comments { get; }
        public List<AttachmentInfo> Attachments { get; }

        [JsonIgnore] public int TotalChecks => UncompletedCheckCount + CompletedCheckCount;
        [JsonIgnore] public int CompletedCheckCount => CheckLists.Sum(x => x.CompletedCheckCount);
        [JsonIgnore] public int UncompletedCheckCount => CheckLists.Sum(x => x.UncompletedCheckCount);
        [JsonIgnore] public IEnumerable<Check> AllChecks => CheckLists.SelectMany(x => x.Checks);
        [JsonIgnore] public double CompletionRate => GetCompletionRate();
        [JsonIgnore] public bool IsCompleted => Math.Abs(this.CompletionRate - 1d) < .01;
        public Color Color(DueStatus status) => TrelloUtil.GetIndicatorColor(CompletionRate, status);

        public override string ToString() => $"{Name} {((TaskTime.Minutes != 0) ? TaskTime.Minutes.ToString() + "m" : string.Empty)} - {Math.Round(CompletionRate * 100, 0)}%";

        private double GetCompletionRate()
        {
            if (_cardStatus == CardStatus.Completed)
                return 1d;
            
            if (TotalChecks == 0)
            {
                return 0d;
            }

            else
                return (double)CompletedCheckCount / (double)TotalChecks;
        }

        internal JobCard(string id, string boardId, string boardName, string name, string trelloListName, IEnumerable<Checklist> checklists, IEnumerable<Comment> comments, CardAreaOfOrigin cardAreaOfOrigin, IProductionPosition productionPosition, CardStatus cardStatus, DateTimeOffset? cardStatusLastUpdated, TimeSpan taskTime)
        {
            this.Id = id;
            this.BoardId = boardId;
            this.BoardName = boardName;
            this.Name = name;
            this.TrelloListName = trelloListName;
            _cardStatus = cardStatus;
            this.AreaOfOrigin = cardAreaOfOrigin;
            this.CheckLists = checklists.ToList();
            this._taskTime = taskTime;
            this.Position = productionPosition;
            this.CardStatusLastUpdated = cardStatusLastUpdated;
            this.Comments = comments.ToList();
            
            this.Attachments = [
                new AttachmentInfo(1.ToString(), "https://media.istockphoto.com/id/152543183/photo/hand-holding-a-red-card.jpg?s=612x612&w=0&k=20&c=n5xIr-ANIHy1s1Sh1z4EgjmLheOCPCywqF0D1GsDmY0=", "hand-holding-a-red-card.jpg"),
                new AttachmentInfo(2.ToString(), "https://thumbs.dreamstime.com/b/two-men-soccer-player-referee-showing-red-card-silhouette-white-background-33185168.jpg", "two-men-soccer-player-referee-showing-red-card-silhouette-white-background-33185168.jpg"),
                new AttachmentInfo(3.ToString(), "https://opengoaaalusa.com/cdn/shop/articles/red_card_in_soccer_1_584fd1ce-0f80-45e9-bfba-3ec4f17e95dc.png?v=1682660140&width=3456", "red_card_in_soccer_1_584fd1ce-0f80-45e9-bfba-3ec4f17e95dc.png"),
                new AttachmentInfo(4.ToString(), "https://cdn-icons-png.flaticon.com/256/7853/7853199.png", "7853199.png")];
        }

        internal JobCard(JobCardObject jobCardObject, ProductionService productionService, IProductionPosition productionPosition, IEnumerable<Comment> comments)
        {
            VanProductionInfo info = productionService.ProductionVans.Values.Single(x => x.Id == jobCardObject.BoardId);
            
            this.Id = jobCardObject.Id;
            this.BoardId = jobCardObject.BoardId;
            this.BoardName = info.Name;
            this.Name = jobCardObject.Name;
            this._taskTime = jobCardObject.TaskTime;
            this.TrelloListName = jobCardObject.TrelloListName;
            _cardStatus = jobCardObject.CardStatus;
            this.CardStatusLastUpdated = jobCardObject.CardStatusLastUpdated;
            this.AreaOfOrigin = jobCardObject.AreaOfOrigin;
            this.Position = productionPosition;

            this.Handover = info.Handover;
            
            if (Position < info.Position)
                this.DueStatus = DueStatus.OverDue;
            else if (Position.Equals(info.Position))
                this.DueStatus = DueStatus.Due;
            else
                this.DueStatus = DueStatus.NotDue;

            List<Checklist> checklists = new List<Checklist>();
            foreach (var chListId in jobCardObject.ChecklistIds)
            {
                if (productionService._checkLists.TryGetValue(chListId, out var checkList))
                {
                    List<Check> checks = new List<Check>();
                    foreach (var chId in checkList.CheckObjectIds)
                    {
                        if (productionService._checks.TryGetValue(chId, out var check))
                        {
                            checks.Add(new Check(check));
                        }
                    }
                    checklists.Add(new Checklist(checkList.Name, checks));
                }
            }

            this.CheckLists = checklists;
            this.Comments = comments.ToList();
            this.Attachments = jobCardObject.Attachments;
        }
    }
}
