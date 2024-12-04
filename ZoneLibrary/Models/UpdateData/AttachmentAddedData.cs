using TrelloDotNet.Model.Webhook;

namespace ZoneProductionLibrary.Models.UpdateData
{
    public class AttachmentAddedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string AttachmentId { get; } = args.Data.Attachment.Id;
        public string AttachmentUrl { get; } = args.Data.Attachment.Url;
        public string FileName { get; } = args.Data.Attachment.Name;
    }

    public class AttachmentRemovedData(WebhookAction args)
    {
        public string BoardId { get; } = args.Data.Board.Id;
        public string CardId { get; } = args.Data.Card.Id;
        public string AttachmentId { get; } = args.Data.Attachment.Id;
    }
}