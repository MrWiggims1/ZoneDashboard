using TrelloDotNet.Model;
using TrelloDotNet.Model.Webhook;

namespace ZoneProductionLibrary.Models.UpdateData
{
    public class CheckDeletedData(WebhookAction args)
    {
        public string CheckId { get; } = args.Data.CheckItem.Id;
        public string CheckListId { get; } = args.Data.Checklist.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string BoardId { get; } = args.Data.Board.Id;
    }

    public class CheckUpdatedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CheckListId { get; } = args.Data.Checklist.Id;
        public string CheckId { get; } = args.Data.CheckItem.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string CheckName { get; } = args.Data.CheckItem.Name;
        public bool IsChecked { get; } = args.Data.CheckItem.State == TrelloDotNet.Model.ChecklistItemState.Complete;
        public DateTimeOffset DateUpdated { get; } = args.Date;
    }
}
