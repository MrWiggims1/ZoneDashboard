using TrelloDotNet.Model;
using Checklist = TrelloDotNet.Model.Checklist;

namespace ZoneProductionLibrary.Models.BoardBreakDown
{
    public class CardBreakDown
    {
        public List<Checklist> Checklists { get; private set; }
        public string Name { get; private set; }
        public string ListName { get; private set; }
        public string ListId { get; private set; }
        public bool JobListIsIgnored { get; private set; } = false;
        
        public TimeSpan TaskTime { get; private set; }
        public CardAreaOfOrigin AreaOfOrigin { get; private set; }
        public IProductionPosition? Position { get; private set; }
        
        
        public CardBreakDown(Card card, IEnumerable<CustomField> customFields, TypeOfVan typeOfVan)
        {
            this.Name = card.Name;
            this.ListName = card.List.Name;
            this.ListId = card.ListId;

            IProductionPosition? position = null;

            if (VanBoardObject.IgnoredJobListsNames.Contains(card.List.Name) ||
                VanBoardObject.IgnoredRedAndYellowCardNames.Contains(card.List.Name) ||
                VanBoardObject.RedCardListNames.Contains(card.List.Name) ||
                VanBoardObject.YellowCardListNames.Contains(card.List.Name))
            {
                JobListIsIgnored = true;
            }
            else
            {
                if (typeOfVan == TypeOfVan.Gen2)
                    Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.JobListName, card.List.Name, out position);
            
                else if (typeOfVan == TypeOfVan.Expo)
                    ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.JobListName, card.List.Name, out position);
            }

            this.Position = position;

            IEnumerable<CustomField> enumerable = customFields.ToList();
            this.TaskTime = TrelloUtil.GetTaskTime(card, enumerable);
            this.AreaOfOrigin = TrelloUtil.ToCardAreaOfOrigin(card, enumerable);
            
            this.Checklists = card.Checklists;
        }
    }
}