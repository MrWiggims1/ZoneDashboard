using System.Drawing;
using System.Text.Json.Serialization;

namespace ZoneProductionLibrary.Models.Boards
{
    public class Checklist
    {
        public string Name { get; internal set; }
        public string Id { get; }
        public List<Check> Checks { get; } = new List<Check>();

        [JsonIgnore] public int CompletedCheckCount => Checks.Count(x => x.IsChecked);
        [JsonIgnore] public int UncompletedCheckCount => Checks.Count(x => !x.IsChecked);
        public double CompletionRate => Checks.Count > 0 ? CompletedCheckCount / (double)Checks.Count : 1d;
        public Color Color(DueStatus status) => TrelloUtil.GetIndicatorColor(CompletionRate, status);

        public override string ToString() => $"{Name}:{Id} - {string.Format("{0:0.00}", CompletionRate * 100)}%";

        public Checklist(TrelloDotNet.Model.Checklist _checkList)
        {
            Name = _checkList.Name;
            Id = _checkList.Id;
            foreach (var _check in _checkList.Items)
            {
                Checks.Add(new Check(_check));
            }
        }

        internal Checklist(string name, IEnumerable<Check> checks)
        {
            Name = name;
            Id = DateTime.Now.Ticks.ToString();
            Checks = checks.ToList();
        }
    }
}
