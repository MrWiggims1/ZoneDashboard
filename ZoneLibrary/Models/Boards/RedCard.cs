using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ZoneProductionLibrary.Models.Boards
{
    public class RedCard : IFilterableCard
    {
        public string Id { get; }
        [JsonIgnore] public string BoardId { get; }
        [JsonIgnore] public string BoardName { get; }
        [JsonIgnore] public TypeOfVan VanType => BoardName.ToVanType().IsGen2() ? TypeOfVan.Gen2 : TypeOfVan.Expo;
        [JsonIgnore] public DateTime? VanHandoverDate { get; }
        [JsonIgnore] public TimeSpan TimeToHandover => (VanHandoverDate.HasValue) ? VanHandoverDate.Value - DateTime.Now : TimeSpan.MinValue;
        public string Name { get; }
        public string Url => $"https://trello.com/c/{Id}/";
        public CardStatus CardStatus { get; }
        [JsonIgnore] public DateTimeOffset? CardStatusLastUpdated { get; private set; }
        public RedFlagIssue RedFlagIssue { get; }
        public CardAreaOfOrigin AreaOfOrigin { get; }
        public DateTimeOffset? CreationDate { get; }
        public List<TrelloMember> Members { get; }
        public List<Comment> Comments { get; }
        public List<AttachmentInfo> Attachments { get; }
        [JsonIgnore] public string MemberNames => GetMemberNames();
        public TimeSpan Age => (CreationDate.HasValue) ? DateTime.Now - CreationDate.Value : TimeSpan.Zero;
        [JsonIgnore] public Color Color => CardStatus is CardStatus.Completed ? Color.Green : Color.Red;

        public override string ToString() => $"{Name} - {Enum.GetName(typeof(CardAreaOfOrigin), AreaOfOrigin)} - {Enum.GetName(typeof(RedFlagIssue), RedFlagIssue)}";

        internal RedCard(string id, string boardId, string boardName, string name, RedFlagIssue issue, CardStatus cardStatus, CardAreaOfOrigin areaOfOrigin, DateTimeOffset handoverDate, DateTimeOffset creationDate,IEnumerable<TrelloMember> members, IEnumerable<Comment> comments, DateTimeOffset? cardStatusLastUpdated)
        {
            Random random = new Random();
            Id = id;
            BoardId = boardId;
            BoardName = boardName;
            Name = name;
            RedFlagIssue = issue;
            CardStatus = cardStatus;
            CardStatusLastUpdated = cardStatusLastUpdated;
            AreaOfOrigin = areaOfOrigin;
            VanHandoverDate = handoverDate.LocalDateTime;
            CreationDate = creationDate;
            Members = members.ToList();
            Comments = comments.ToList();
            Attachments = [
                new AttachmentInfo(1.ToString(), "https://media.istockphoto.com/id/152543183/photo/hand-holding-a-red-card.jpg?s=612x612&w=0&k=20&c=n5xIr-ANIHy1s1Sh1z4EgjmLheOCPCywqF0D1GsDmY0=", "hand-holding-a-red-card.jpg"),
                new AttachmentInfo(2.ToString(), "https://thumbs.dreamstime.com/b/two-men-soccer-player-referee-showing-red-card-silhouette-white-background-33185168.jpg", "two-men-soccer-player-referee-showing-red-card-silhouette-white-background-33185168.jpg"),
                new AttachmentInfo(3.ToString(), "https://opengoaaalusa.com/cdn/shop/articles/red_card_in_soccer_1_584fd1ce-0f80-45e9-bfba-3ec4f17e95dc.png?v=1682660140&width=3456", "red_card_in_soccer_1_584fd1ce-0f80-45e9-bfba-3ec4f17e95dc.png"),
                new AttachmentInfo(4.ToString(), "https://cdn-icons-png.flaticon.com/256/7853/7853199.png", "7853199.png")];
        }

        internal RedCard(RedCardObject redCardObject, VanProductionInfo productionInfo, IEnumerable<TrelloMember> members, IEnumerable<Comment> comments)
        {
            Id = redCardObject.Id;
            BoardId = redCardObject.BoardId;
            Name = redCardObject.Name;
            RedFlagIssue = redCardObject.RedFlagIssue;
            CardStatus = redCardObject.CardStatus;
            CardStatusLastUpdated = redCardObject.CardStatusLastUpdated;
            AreaOfOrigin = redCardObject.AreaOfOrigin;
            CreationDate = redCardObject.CreationDate;

            VanHandoverDate = productionInfo.Handover?.LocalDateTime;
            BoardName = productionInfo.Name;
            Members = members.ToList();
            Comments = comments.ToList();
            Attachments = redCardObject.Attachments;
        }

        internal RedCard(RedCardObject redCardObject, string boardName, DateTimeOffset? handoverDate, IEnumerable<TrelloMember> members, IEnumerable<Comment> comments)
        {
            Id = redCardObject.Id;
            BoardId = redCardObject.BoardId;
            Name = redCardObject.Name;
            RedFlagIssue = redCardObject.RedFlagIssue;
            CardStatus = redCardObject.CardStatus;
            CardStatusLastUpdated = redCardObject.CardStatusLastUpdated;
            AreaOfOrigin = redCardObject.AreaOfOrigin;
            CreationDate = redCardObject.CreationDate;

            VanHandoverDate = handoverDate?.LocalDateTime;
            BoardName = boardName;
            Members = members.ToList();
            Comments = comments.ToList();
            Attachments = redCardObject.Attachments;
        }

        string GetMemberNames()
        {
            List<string> memberNames = Members.Select(x => x.Username).ToList();

            foreach (Comment comment in Comments)
            {
                memberNames.Add(comment.CreatorMember.Username);
                foreach (string word in Regex.Split(comment.Content, @"\s+").Where(s => s != string.Empty))
                {
                    if(word.StartsWith('@'))
                        memberNames.Add(new string(word.Skip(1).ToArray()));
                }
            }

            memberNames = memberNames.Distinct().ToList();

            return string.Join(", ", memberNames);
        }
    }
}
