namespace ZoneProductionLibrary.Models.Stats
{
    public class DateDataItem
    {
        public DateTime Key { get; set; }
        public decimal? Value { get; set; }
        public object? Info { get; set; }

        public override string ToString() => $"{Key}:{Value}";

        public DateDataItem(DateTime key, decimal? value)
        {
            Key = key + DateTimeOffset.Now.Offset;
            Value = value;
        }

        public DateDataItem(DateTime key, decimal? value, object info)
        {
            Key = key + DateTimeOffset.Now.Offset;
            Value = value;
            Info = info;
        }
    }
}
