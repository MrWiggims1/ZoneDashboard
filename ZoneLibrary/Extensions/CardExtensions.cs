using ZoneProductionLibrary.Models.Stats;

namespace ZoneProductionLibrary.Extensions
{
    public static class CardExtensions
    {
        public static DueStatus GetTargetStatus(this IEnumerable<JobCard> cards, IProductionPosition vanPosition)
        {
            if (cards.All(x => x.DueStatus == DueStatus.OverDue))
                return DueStatus.OverDue;

            else if (cards.Any(x => x.DueStatus is DueStatus.Due or DueStatus.OverDue))
                return DueStatus.Due;

            else
                return DueStatus.NotDue;
        }

        public static double GetCompletionRate(this IEnumerable<JobCard> cards, int? roundingDigits = null)
        {
            double totalScore = cards.Sum(x => x.CompletionRate);

            double total = cards.Count();

            if (total == 0d)
                return 0d;

            double value = totalScore / total;

            return roundingDigits is null ? value : double.Round(100 * value, roundingDigits.Value);
        }
        
        public static IOrderedEnumerable<(DateTimeOffset? date, decimal change)> GetCompletionRateHistory(this IEnumerable<JobCard> cards)
        {
            List<(DateTimeOffset? date, decimal change)> results = [];

            IEnumerable<JobCard> jobCards = cards.ToList();

            decimal divisor = jobCards.Sum(x => x.TaskTime != TimeSpan.Zero ? (decimal)x.TaskTime.TotalMinutes : 30M);
            
            foreach (JobCard card in jobCards)
            {
                decimal taskMinutes = card.TaskTime != TimeSpan.Zero ? (decimal)card.TaskTime.TotalMinutes : 30M; 
                
                if (card.TotalChecks > 1)
                {
                    List<Check> checksToMap = card.AllChecks.ToList();
                        
                    if (card.CardStatus == CardStatus.Completed)
                    {
                        checksToMap = checksToMap.Where(x => x.LastUpdated < card.CardStatusLastUpdated).ToList();
                    }
                    
                    foreach (Check check in checksToMap)
                    {
                        if (check.IsChecked)
                        {
                            results.Add((check.LastUpdated, (taskMinutes / card.TotalChecks) / divisor));
                        }
                    }

                    if (checksToMap.Count != card.AllChecks.Count())
                    {
                        results.Add((card.CardStatusLastUpdated, (taskMinutes * (card.AllChecks.Count() - checksToMap.Count) / card.TotalChecks) / divisor )); 
                    }
                }
                else if (card.CardStatus == CardStatus.Completed)
                {
                    results.Add((card.CardStatusLastUpdated, taskMinutes / divisor));
                }
            }

            return results.OrderBy(x => x.date);
        }

        public static Dictionary<string, decimal> GetWorkCompletedByBay(this IEnumerable<JobCard> jobCards, TypeOfVan vanType, IEnumerable<(DateTimeOffset date, IProductionPosition position)> vanPositionHistory)
        {
            Dictionary<string, decimal> results = new Dictionary<string, decimal>();
            IEnumerable<(DateTimeOffset date, IProductionPosition position)> positionHistory = vanPositionHistory.ToList();
            IEnumerable<JobCard> enumerable = jobCards.ToList();

            if (!enumerable.Any())
                return results;
            
            results.Add(new PreProduction().PositionName, 0);

            if (vanType == TypeOfVan.Gen2)
            {
                foreach (Gen2ProductionPosition pos in Gen2ProductionPosition.GetAll())
                {
                    results.Add(pos.PositionName, 0);
                }
            }
            else
            {
                foreach (ExpoProductionPosition pos in ExpoProductionPosition.GetAll())
                {
                    results.Add(pos.PositionName, 0);
                }
            }
            
            results.Add(new PostProduction().PositionName, 0);

            decimal divisor = enumerable.Sum(x => x.TaskTime != TimeSpan.Zero ? (decimal)x.TaskTime.TotalMinutes : 30M);

            foreach (JobCard card in enumerable)
            {
                decimal taskMinutes = card.TaskTime != TimeSpan.Zero ? (decimal)card.TaskTime.TotalMinutes : 30M;

                if (card.TotalChecks > 0)
                {
                    List<Check> checksToMap = card.AllChecks.ToList();
                        
                    if (card.CardStatus == CardStatus.Completed)
                    {
                        checksToMap = checksToMap.Where(x => x.LastUpdated < card.CardStatusLastUpdated).ToList();
                    }
                    
                    foreach (Check check in checksToMap)
                    {
                        if (check.IsChecked && check.LastUpdated.HasValue)
                        {
                            IProductionPosition pos = positionHistory.GetPosition(check.LastUpdated.Value);
                        
                            results[pos.PositionName] += taskMinutes / card.TotalChecks / divisor;
                        }
                    }

                    if (checksToMap.Count != card.AllChecks.Count() && card.CardStatusLastUpdated.HasValue)
                    {
                        IProductionPosition pos = positionHistory.GetPosition(card.CardStatusLastUpdated.Value);
                        
                        results[pos.PositionName] += (taskMinutes * (card.AllChecks.Count() - checksToMap.Count) / card.TotalChecks) / divisor;
                    }
                }
                else
                {
                    if (card.IsCompleted && card.CardStatusLastUpdated.HasValue)
                    {
                        IProductionPosition pos = positionHistory.GetPosition(card.CardStatusLastUpdated.Value);
                        
                        results[pos.PositionName] += taskMinutes / divisor;
                    }
                }
            }

            return results;
        }
        
        public static IEnumerable<DateDataItem> RedCardCountByLocalDateData(this IEnumerable<RedCard> redCards, bool ignoreWeekends)
        {
            IEnumerable<RedCard> enumerable = redCards.ToList();
            
            if (!enumerable.Any())
                return [];
            
            List<DateDataItem> results = [];

            Dictionary<DateTime, int> data = new Dictionary<DateTime, int>();

            foreach (RedCard redCard in enumerable)
            {
                if(!redCard.CreationDate.HasValue)
                    continue;

                DateTime date = redCard.CreationDate.Value.LocalDateTime.Date;

                if (!ignoreWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    if(!data.ContainsKey(date))
                        data.Add(date, 0);
                
                    data[date] ++;
                }
            }

            for (DateTime date = data.Keys.Min(); date <= data.Keys.Max(); date += TimeSpan.FromDays(1))
            {
                if (!ignoreWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    if (data.ContainsKey(date))
                    {
                        results.Add(new DateDataItem(date, data[date]));
                    }

                    else
                        results.Add(new DateDataItem(date, 0));
                }
            }

            return results;
        }

        public static IEnumerable<DateDataItem> RedCardCountByLocalDateData(this IEnumerable<RedCard> redCards, TimeSpan timeStep, DateTime minDate, DateTime maxDate)
        {
            Dictionary<DateTime, int> countByDay = [];
            List<DateDataItem> results = [];

            minDate = minDate.Date;
            maxDate = maxDate.Date;

            foreach (RedCard card in redCards)
            {
                if(!card.CreationDate.HasValue)
                    continue;

                DateTime date = card.CreationDate.Value.LocalDateTime - TimeSpan.FromTicks(card.CreationDate.Value.LocalDateTime.TimeOfDay.Ticks % timeStep.Ticks);
                
                if(date > maxDate)
                    continue;
                
                if(date < minDate)
                    continue;

                if (countByDay.ContainsKey(date))
                    countByDay[date]++;
                else
                    countByDay.Add(date, 1);
            }

            if (countByDay.Count == 0)
                return [];
            
            var startDate = minDate - TimeSpan.FromTicks(minDate.TimeOfDay.Ticks % timeStep.Ticks); 

            for (; startDate < maxDate; startDate += timeStep)
            {
                if(countByDay.TryGetValue(startDate, out int dataPoint))
                    results.Add(new DateDataItem(startDate, dataPoint));
                
                else if(startDate.DayOfWeek is DayOfWeek.Saturday or  DayOfWeek.Sunday)
                    results.Add(new DateDataItem(startDate, null));
                
                else
                    results.Add(new DateDataItem(startDate, 0));
            }

            return results;
        }
        
        public static IEnumerable<DataItem> RedCardCountByLocalDateData(this IEnumerable<RedCard> redCards, bool ignoreWeekends, string dateTimeStringFormat, DateTime? minDate = null, DateTime? maxDate = null)
        {
            IEnumerable<RedCard> enumerable = redCards.ToList();
            
            if (!enumerable.Any())
            {
                if(minDate is null && maxDate is null)
                    return [];
            }
            
            List<DataItem> results = [];

            Dictionary<DateTime, int> data = new Dictionary<DateTime, int>();

            foreach (RedCard redCard in enumerable)
            {
                if(!redCard.CreationDate.HasValue)
                    continue;

                DateTime date = redCard.CreationDate.Value.LocalDateTime.Date;

                if (!ignoreWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    if(!data.ContainsKey(date))
                        data.Add(date, 0);
                
                    data[date] ++;
                }
            }
            
            if (minDate.HasValue)
                minDate = minDate.Value.Date;
            else
                minDate = data.Keys.Min();
            
            if (maxDate.HasValue)
                maxDate = maxDate.Value.Date;
            else
                maxDate = data.Keys.Max();

            for (DateTime date = minDate.Value; date <= maxDate; date += TimeSpan.FromDays(1))
            {
                if (!ignoreWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
                {
                    if (data.ContainsKey(date))
                    {
                        results.Add(new DataItem(date.ToString(dateTimeStringFormat), data[date]));
                    }

                    else
                        results.Add(new DataItem(date.ToString(dateTimeStringFormat), 0));
                }
            }

            return results;
        }

        public static IEnumerable<DataItem> RedCardByTypeData(this IEnumerable<RedCard> redCards)
            => redCards.RedCardByTypeData(Enum.GetValues<RedFlagIssue>());
        
        public static IEnumerable<DataItem> RedCardByTypeData(this IEnumerable<RedCard> redCards, IEnumerable<RedFlagIssue> issueTypes)
        {
            List<DataItem> data = new List<DataItem>();
            IEnumerable<RedCard> enumerable = redCards.ToList();

            foreach (RedFlagIssue type in issueTypes)
            {
                int count = enumerable.Count(x => x.RedFlagIssue == type);
                
                if(count > 0)
                    data.Add(new DataItem(type, count, count));
            }

            return data;
        }

        public static TimeSpan TotalTime(this IEnumerable<JobCard> jobCards)
        {
            return TimeSpan.FromMinutes(jobCards.Sum(x => x.TaskTime.TotalMinutes));
        }
        
        
        public static TimeSpan TotalTimeRemaining(this IEnumerable<JobCard> jobCards)
        {
            return TimeSpan.FromMinutes(jobCards.Sum(x => x.RemainingTaskTime.TotalMinutes));
        }
    }
}
