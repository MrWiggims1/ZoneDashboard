using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ZoneProductionLibrary.Models.Boards
{
    public class YellowCard : IFilterableCard
    {
        public string Id { get; }
        [JsonIgnore] public string BoardId { get; }
        [JsonIgnore] public string BoardName { get; }
        [JsonIgnore] public TypeOfVan VanType => BoardName.ToVanType().IsGen2() ? TypeOfVan.Gen2 : TypeOfVan.Expo;
        public string Name { get; }
        public string PartName => Name.Split(' ').First();
        public string Url => $"https://trello.com/c/{Id}/";
        public CardStatus CardStatus { get; }
        public DateTimeOffset? CardStatusLastUpdated { get; private set; }
        public CardAreaOfOrigin AreaOfOrigin { get; }
        public DateTimeOffset? CreationDate { get; }
        public List<Comment> Comments { get; }
        public List<AttachmentInfo> Attachments { get; }
        [JsonIgnore] public TimeSpan Age => (CreationDate.HasValue) ? DateTime.Now - CreationDate.Value : TimeSpan.Zero;
        [JsonIgnore] public Color Color => CardStatus is CardStatus.Completed ? Color.Green : Color.Red;

        public override string ToString() => $"{Name} - {Enum.GetName(typeof(CardAreaOfOrigin), AreaOfOrigin)}";

        internal YellowCard(string id, string boardId, string boardName, string name, CardStatus cardStatus, CardAreaOfOrigin areaOfOrigin, DateTimeOffset creationDate, IEnumerable<Comment> comments, DateTimeOffset? cardStatusLastUpdated)
        {
            Random random = new Random();
            Id = id;
            BoardId = boardId;
            BoardName = boardName;
            Name = name;
            CardStatus = cardStatus;
            CardStatusLastUpdated = cardStatusLastUpdated;
            AreaOfOrigin = areaOfOrigin;
            CreationDate = creationDate;
            Comments = comments.ToList();
            Attachments = [
                new AttachmentInfo(5.ToString(), "https://t4.ftcdn.net/jpg/04/27/31/03/360_F_427310311_NA8QjnKqLApxo6h4EgkZCeYTr6VRuoP8.jpg", "hand-holding-a-yellow-card.jpg"),
                new AttachmentInfo(6.ToString(), "https://img.freepik.com/free-photo/close-up-woman-holding-up-yellow-business-card_23-2148295704.jpg", "yellowCard.jpg")];
        }

        internal YellowCard(RedCardObject redCardObject, VanProductionInfo productionInfo, IEnumerable<Comment> comments)
        {
            Id = redCardObject.Id;
            BoardId = redCardObject.BoardId;
            Name = redCardObject.Name;
            CardStatus = redCardObject.CardStatus;
            CardStatusLastUpdated = redCardObject.CardStatusLastUpdated;
            AreaOfOrigin = redCardObject.AreaOfOrigin;
            CreationDate = redCardObject.CreationDate;

            BoardName = productionInfo.Name;
            Comments = comments.ToList();
            Attachments = redCardObject.Attachments;
        }

        internal YellowCard(RedCardObject redCardObject, string boardName, IEnumerable<Comment> comments)
        {
            Id = redCardObject.Id;
            BoardId = redCardObject.BoardId;
            Name = redCardObject.Name;
            CardStatus = redCardObject.CardStatus;
            CardStatusLastUpdated = redCardObject.CardStatusLastUpdated;
            AreaOfOrigin = redCardObject.AreaOfOrigin;
            CreationDate = redCardObject.CreationDate;

            BoardName = boardName;
            Comments = comments.ToList();
            Attachments = redCardObject.Attachments;
        }
    }
}
