namespace ZoneProductionLibrary.Models.Stats
{
    public class CategoryDataItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"{Key}:{Value}";

        public CategoryDataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
