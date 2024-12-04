namespace ZoneProductionLibrary.Models.Stats
{
    public class BoxPlotDataItem
    {
        public object Key { get; }
        public decimal Max { get; }
        public decimal Min { get; }
        public decimal Mean { get; }
        public decimal Q1 { get; }
        public decimal Q2 { get; }
        public decimal Q3 { get; }

        public List<decimal> Outliers { get; set; }
        public List<decimal> Inliers { get; set; }

        public override string ToString() => $"{this.Key}";

        public BoxPlotDataItem(object key, IEnumerable<decimal> values, decimal multiplier)
        {
            decimal[] array = values.Order().ToArray();
            
            if(array.Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");
            
            this.Key = key;

            this.Max = array.Max();
            this.Min = array.Min();
            int halfWay = array.Length / 2;

            if (array.Length == 1)
            {
                this.Q1 = array[0];
                this.Q2 = array[0];
                this.Q3 = array[0];
            }

            else if (array.Length % 2 == 0)
            {
                this.Q2 = (array[halfWay - 1] + array[halfWay]) / 2;
                
                int quarterWay = halfWay / 2;
                
                if (halfWay % 2 == 0)
                {
                    this.Q1 = (array[quarterWay - 1] + array[quarterWay]) / 2;
                    this.Q3 = (array[halfWay + quarterWay - 1] + array[halfWay + quarterWay]) / 2;
                }
                else
                {
                    this.Q1 = array[quarterWay];
                    this.Q3 = array[halfWay + quarterWay];
                }
            }

            else
            {
                this.Q2 = array[halfWay];
                
                int quarterWay = halfWay / 2;
                
                if ((array.Length - 1) % 4 == 0)
                {
                    this.Q1 = (array[quarterWay - 1] * .25m) + (array[quarterWay] * .75m);
                    this.Q3 = (array[halfWay + quarterWay - 1] * .75m) + (array[halfWay + quarterWay] * .25m);
                }
                else if ((array.Length - 3) % 4 == 0)
                {

                    this.Q1 = (array[quarterWay] * .75m) + (array[quarterWay + 1] * .25m);
                    this.Q3 = (array[halfWay + quarterWay] * .25m) + (array[halfWay + quarterWay + 1] * .75m);
                }
            }

            decimal IQR = this.Q3 - this.Q2;
            decimal lowerLimit = Q1 - 1.5m * IQR;
            decimal upperLimit = Q3 + 1.5m * IQR;

            this.Outliers = [];
            this.Inliers = [];

            foreach (decimal point in array)
            {
                if(point > upperLimit || point < lowerLimit)
                    this.Outliers.Add(point);
                
                else
                    this.Inliers.Add(point);
            }

            this.Mean = this.Inliers.Count != 0 ? this.Inliers.Average() : array.Average();

            this.Min = Math.Max(this.Min, lowerLimit);
            this.Max = Math.Min(this.Max, upperLimit);

            this.Min = Math.Round(this.Min * multiplier,   1);
            this.Max = Math.Round(this.Max * multiplier,   1);
            
            this.Mean = Math.Round(this.Mean * multiplier, 1);
            this.Q1 = Math.Round(this.Q1 * multiplier,     1);
            this.Q2 = Math.Round(this.Q2 * multiplier,     1);
            this.Q3 = Math.Round(this.Q3 * multiplier,     1);
        }
    }
}
