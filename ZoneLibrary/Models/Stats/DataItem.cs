namespace ZoneProductionLibrary.Models.Stats
{
    public class DataItem
    {
        public object Key { get; set; }
        public Decimal Value { get; set; }
        public object? Extra { get; set; }

        public override string ToString() => $"{Key}:{Value}";

        public DataItem(object key, decimal value)
        {
            Key = key;
            Value = value;
        }
        
        public DataItem(object key, decimal value, object extra)
        {
            Key = key;
            Value = value;
            Extra = extra;
        }
    }
}
