using TrelloDotNet.Model.Webhook;

namespace ZoneProductionLibrary.Models.UpdateData
{
    public class CheckListDeletedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string CheckListId { get; } = args.Data.Checklist.Id;
    }

    public class CheckListUpdatedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CheckListId { get; } = args.Data.Checklist.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string CheckListName { get; } = args.Data.Checklist.Name;
    }

    public class CheckListCreatedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string CheckListId { get; } = args.Data.Checklist.Id;
        public string CheckListName { get; } = args.Data.Checklist.Name;
    }

}
