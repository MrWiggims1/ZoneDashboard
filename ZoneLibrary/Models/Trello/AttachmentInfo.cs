using Newtonsoft.Json;
using TrelloDotNet.Model;
using ZoneProductionLibrary.Models.UpdateData;

namespace ZoneProductionLibrary.Models.Trello
{
    

    public class AttachmentInfo
    {
        public string Id { get; }
        public string Url { get; }
        public string FileName { get; }

        [JsonIgnore] public string FilePath => $"attachments/{Id}-{FileName}";

        public AttachmentInfo(string id, string url, string fileName)
        {
            Id = id;
            Url = url;
            FileName = fileName;
        }

        public AttachmentInfo(Attachment attachment)
        {
            Id = attachment.Id;
            Url = attachment.Url;
            FileName = attachment.FileName;
        }

        public AttachmentInfo(AttachmentAddedData data)
        {
            Id = data.AttachmentId;
            Url = data.AttachmentUrl;
            FileName = data.FileName;
        }
    }
}
