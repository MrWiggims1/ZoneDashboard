using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using ZoneProductionLibrary.Models.ProductionPosition;

namespace ZoneProductionLibrary.Extensions
{
    public static class ProductionExtensions
    {
        public static bool IsFirst(this IProductionPosition position)
        {
            if (position is Gen2ProductionPosition gen2)
                return gen2.Equals(Gen2ProductionPosition.First);
            
            else if (position is ExpoProductionPosition expo)
                return expo.Equals(ExpoProductionPosition.First);

            else
                return false;
                    
        }
        
        public static bool IsLast(this IProductionPosition position)
        {
            if (position is Gen2ProductionPosition gen2)
                return gen2.Equals(Gen2ProductionPosition.Last);
            
            else if (position is ExpoProductionPosition expo)
                return expo.Equals(ExpoProductionPosition.Last);

            else
                return false;
                    
        }

        public static List<(DateTimeOffset date, IProductionPosition)> ToPositionHistory(this IEnumerable<TrelloAction> actions, List<List>? lists)
        {
            if (lists is null)
                throw new NullReferenceException("Lists must be loaded for position history.");
            
            List<(DateTimeOffset date, IProductionPosition)> positionHistory = [];

            List<TrelloAction> moves = actions.Where(x => x.Type == "updateCard" && x.Data.ListAfter != null && x.Data.ListBefore != null).OrderBy(x => x.Date).ToList();

            foreach (TrelloAction move in moves)
            {
                List list;

                if (lists.Count(x => x.Id == move.Data.ListAfter.Id) == 1)
                    list = lists.Where(x => x.Id == move.Data.ListAfter.Id).Single();

                else
                    continue;

                string listName = list.Name;

                if (listName == "SCHEDULED VANS (50x)" || listName == "SCHEDULED EXPO VANS")
                    positionHistory.Add((move.Date, new PreProduction()));

                else if (Gen2ProductionPosition.Positions.Any(x => x.name == listName) && Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.LineMoveName,
                             listName,
                             out IProductionPosition? gen2Position))
                    positionHistory.Add((move.Date, gen2Position)!);

                else if (ExpoProductionPosition.Positions.Any(x => x.name == listName) && ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.LineMoveName,
                             listName,
                             out IProductionPosition? expPosition))
                    positionHistory.Add((move.Date, expPosition)!);

                else
                    positionHistory.Add((move.Date, new PostProduction()));
            }

            return positionHistory;
        }
        
        public static IProductionPosition GetPosition(this IEnumerable<(DateTimeOffset date, IProductionPosition position)> PositionHistory, DateTimeOffset date)
        {
            if (PositionHistory.Count() == 0)
                return new PreProduction();

            else if (date < PositionHistory.First().date)
                return new PreProduction();

            else if (date > PositionHistory.Last().date)
                return PositionHistory.Last().position;

            else
            {
                return PositionHistory.TakeWhile(x => x.date < date).Last().position;
            }
        }

        public static IProductionPosition GetPosition(this IEnumerable<(DateTimeOffset date, IProductionPosition position)> PositionHistory, DateTime date)
            => PositionHistory.GetPosition(new DateTimeOffset(date.ToUniversalTime(), TimeSpan.Zero));

        public static (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(this IEnumerable<(DateTimeOffset date, IProductionPosition position)> PositionHistory, IProductionPosition position)
        {
            if (!PositionHistory.Any(x => x.position.Equals(position)))
                return null;

            else
            {
                (DateTimeOffset start, DateTimeOffset? end) result;
                var remainingPos = PositionHistory.SkipWhile(x => x.position < position);

                result.start = remainingPos.First().date;

                if (remainingPos.Count() > 1)
                    result.end = remainingPos.ElementAt(1).date;

                else
                    result.end = null;

                return result;
            }
        }

        public static bool ContainsPositionWithinDate(this IEnumerable<(DateTimeOffset date, IProductionPosition position)> PositionHistory, IProductionPosition position, DateTimeOffset start, DateTimeOffset end)
        {
            if (!PositionHistory.All(x => x.position is PreProduction || x.position is PostProduction || x.position.GetType() == position.GetType()))
                return false;
            
            if (!PositionHistory.Any(x => x.position.Equals(position)))
            {
                if(position is PreProduction)
                    return true;
                else
                    return false;
            }

            else
            {
                var desiredPos = PositionHistory.SkipWhile(x => x.position < position).First();
                var nextPos = PositionHistory.SkipWhile(x => x.position < position || x.position.Equals(position)).FirstOrDefault();

                if (end <= desiredPos.date)
                    return false;

                else if (nextPos.Equals(default))
                    return true;

                else if (start >= nextPos.date)
                    return false; 

                else 
                    return true;
            }
        }
    }
}