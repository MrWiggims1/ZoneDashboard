namespace ZoneProductionLibrary.Models.Stats
{
    public class DateDataRange
    {
        public DateTime Key { get; set; }
        public decimal TopValue { get; set; }
        public decimal LowerValue { get; set; }
        public object? Info { get; set; }

        public override string ToString() => $"{Key}:{TopValue} - {LowerValue}";

        public DateDataRange(DateTime key, decimal lowervalue, decimal topvalue)
        {
            Key = key;
            LowerValue = lowervalue;
            TopValue = topvalue;
        }

        public DateDataRange(DateTime key, decimal lowervalue, decimal topvalue, object info)
        {
            Key = key;
            LowerValue = lowervalue;
            TopValue = topvalue;
            Info = info;
        }
    }
}
