namespace ZoneProductionLibrary.Extensions
{
    public static class ColorExtensions
    {
        public static readonly List<string> ColorStrings =
        [
            "#a61c00", "#cc0000", "#e69138", "#f1c232", "#6aa84f", "#45818e", "#3c78d8", "#3d85c6", "#674ea7", "#a64d79",
            "#cc4125", "#e06666", "#f6b26b", "#ffd966", "#93c47d", "#76a5af", "#6d9eeb", "#6fa8dc", "#8e7cc3", "#c27ba0",
            "#dd7e6b", "#ea9999", "#f9cb9c", "#ffe599", "#b6d7a8", "#a2c4c9", "#a4c2f4", "#9fc5e8", "#b4a7d6", "#d5a6bd"
        ];
        
        public static string ToChartColor(this RedFlagIssue issue)
            => ColorStrings.ElementAt((int)issue % ColorStrings.Count);
        
        public static string ToChartColor(this CardAreaOfOrigin area)
            => ColorStrings.ElementAt((int)area % ColorStrings.Count);
        
        public static string ToHex(this System.Drawing.Color c)
        {
            var s = $"#{c.R:X2}{c.G:X2}{c.B:X2}";

            return s;
        }
    }
}