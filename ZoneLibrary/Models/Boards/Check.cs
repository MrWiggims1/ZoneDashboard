using Newtonsoft.Json;
using System.Drawing;
using TrelloDotNet.Model;

namespace ZoneProductionLibrary.Models.Boards
{
    public class Check
    {
        public bool IsChecked { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public string Name { get; }
        public string Id { get; }
        public Color Color(DueStatus status) => TrelloUtil.GetIndicatorColor(IsChecked, status); //TODO Implement 

        public override string ToString() => $"{Name}:{Id}";

        internal Check(CheckObject check)
        {
            Name = check.Name;
            IsChecked = check.IsChecked;
            Id = check.Id;
            LastUpdated = check.LastModified;
        }
        
        public Check(ChecklistItem _checkItem)
        {
            Name = _checkItem.Name;
            IsChecked = _checkItem.State == ChecklistItemState.Complete;
            Id = _checkItem.Id;
        }

        internal Check(string name, bool isChecked)
        {
            Name = name;
            IsChecked = isChecked;
            Id = _checkId.ToString();

            _checkId++;
        }
        
        private static int _checkId = 0;
    }
}
