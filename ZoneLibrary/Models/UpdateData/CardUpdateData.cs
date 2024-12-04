using TrelloDotNet.Model.Webhook;

namespace ZoneProductionLibrary.Models.UpdateData
{
    public class CardUpdatedData(WebhookAction args)
    {
        public string BoardId { get; set; } = args.Data.Board.Id;
        public string CardId { get; set; } = args.Data.Card.Id;
        public string CardName { get; set; } = args.Data.Card.Name;
    }
}
