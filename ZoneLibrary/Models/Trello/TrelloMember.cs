
using Newtonsoft.Json;
using TrelloDotNet.Model;

namespace ZoneProductionLibrary.Models.Trello
{
    public class TrelloMember
    {
        [JsonIgnore] public string Id { get; set; }
        [JsonIgnore] public string OrgId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; }

        public TrelloMember(Member member, string orgId)
        {
            Id = member.Id;
            FullName = member.FullName;
            Username = member.Username;
            OrgId = orgId;
            AvatarUrl = member.AvatarUrl30;
        }

        public TrelloMember(string id, string fullName, string username, string orgId)
        {
            Id = id;
            FullName = fullName;
            Username = username;
            OrgId = orgId;
            AvatarUrl = "https://www.radzen.com/assets/radzen-logo-top-b2d6e9dcacf7d344bbab515b8748c5f4d702c6c5bfc349bd9ff9003016a3a6ee.svg";
        }

        public override string ToString() => $"{Username} {Id}";
    }
}
