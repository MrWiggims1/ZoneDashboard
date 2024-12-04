using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class CompareReport
    {
        [JsonInclude]
        public bool Pass { get; set; }
        
        [JsonInclude]
        public Dictionary<string, List<string>> Issues { get; set; }

        public CompareReport(string objectString)
        {
            Pass = true;
            Issues = [];
            
            Issues.Add(objectString, []);
        }

        public void Concat(CompareReport? report)
        {
            if(report is null)
                return;
            
            Pass = !(!Pass || !report.Pass);

            foreach (string key in report.Issues.Keys)
            {
                if(report.Issues[key].Count == 0)
                    continue;
                
                if (Issues.ContainsKey(key))
                    Issues.Add($"{key} - duplicate", report.Issues[key]);
                
                else
                    Issues.Add(key, report.Issues[key]);
            }
        }
    }
    
}