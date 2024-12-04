using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetBoardOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using ZoneProductionDashBoard;

namespace ZoneProductionLibrary.Models.BoardBreakDown
{
    public class BoardBreakDown
    {
        public List<CardBreakDown> Cards { get; } = [];
        public List<(string Id, string Name)> Lists { get; } = [];
        public Dictionary<IProductionPosition, List<CardBreakDown>> CardsByPosition { get; private set; } = [];
        public Dictionary<string, List<CardBreakDown>> CardsByListId { get; private set; } = [];
        
        
        
        private BoardBreakDown() { }
        
        public static async Task<BoardBreakDown> CreateBreakDown(string id, TypeOfVan? typeOfVan = null)
        {
            TrelloClientOptions clientOptions = new TrelloClientOptions
            {
                AllowDeleteOfBoards = false,
                AllowDeleteOfOrganizations = false,
                IncludeCustomFieldsInCardGetMethods = true,
                IncludeAttachmentsInCardGetMethods = false,
                MaxRetryCountForTokenLimitExceeded = 3
            };

            TrelloClient trelloClient = new TrelloClient(DashboardConfig.TrelloApiKey, DashboardConfig.TrelloUserToken,
                                                         clientOptions);

            GetCardOptions getCardOptions = new GetCardOptions()
            {
                IncludeChecklists       = true,
                ChecklistFields         = ChecklistFields.All,
                IncludeList             = true,
                IncludeBoard            = true,
                BoardFields             = new BoardFields(BoardFieldsType.Name),
                IncludeCustomFieldItems = true,
                CardFields = new CardFields(
                    CardFieldsType.Name,
                    CardFieldsType.ChecklistIds,
                    CardFieldsType.Closed,
                    CardFieldsType.ListId,
                    CardFieldsType.MemberIds
                    )
            };

            BoardBreakDown breakDown = new BoardBreakDown();

            Board? board = await trelloClient.GetBoardAsync(id);

            if (typeOfVan is null)
            {
                typeOfVan = board.Name.ToVanType().IsGen2() ? TypeOfVan.Gen2 : TypeOfVan.Expo;
            }

            if (board is null)
                throw new Exception($"Could not find board with id {id}");
            
            List<CustomField>? customFields = await trelloClient.GetCustomFieldsOnBoardAsync(id);
            List<Card>? cards = await trelloClient.GetCardsOnBoardAsync(id, getCardOptions);
            List<List>? lists = await trelloClient.GetListsOnBoardAsync(id);

            if (customFields is null || cards is null || lists is null)
                throw new Exception($"Exception trying to make board breakdown for {board.Name}");

            foreach (List list in lists)
            {
                breakDown.Lists.Add((list.Id, list.Name));
            }

            foreach (Card card in cards)
            {
                breakDown.Cards.Add(new CardBreakDown(card, customFields, typeOfVan.Value));
            }

            if (typeOfVan == TypeOfVan.Gen2)
            {
                foreach (Gen2ProductionPosition position in Gen2ProductionPosition.GetAll())
                {
                    breakDown.CardsByPosition.Add(position, breakDown.Cards.Where(x => 
                                                      x.Position is not null && x.Position.Equals(position)).ToList());
                }
            }
            else
            {
                foreach (ExpoProductionPosition position in ExpoProductionPosition.GetAll())
                {
                    breakDown.CardsByPosition.Add(position, breakDown.Cards.Where(x =>
                                                      x.Position is not null && x.Position.Equals(position)).ToList());
                }
            }

            foreach ((string Id, string Name) list in breakDown.Lists)
            {
                breakDown.CardsByListId.Add(list.Id, breakDown.Cards.Where(x => x.ListId == list.Id).ToList());
            }

            return breakDown;
        }
    }
}