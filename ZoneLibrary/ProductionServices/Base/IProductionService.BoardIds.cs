using System.Diagnostics;
using TrelloDotNet.Model;

namespace ZoneProductionLibrary.ProductionServices.Base
{
    public partial interface IProductionService
    {
        public IEnumerable<string> RequiredBoardIds()
        {
            int daysOfPrevHandovers = Debugger.IsAttached ? 7 : 31;
            
            List<string> ids = [];
            
            ids.AddRange(ProductionVanIds(true));
            ids.AddRange(GetNextGen2HanoverIds(10));
            ids.AddRange(GetNextExpoHanoverIds(10));
            ids.AddRange(GetVanIdsByLocalHandoverDates(DateTime.Now - TimeSpan.FromDays(daysOfPrevHandovers), DateTime.Now));

            return ids.Distinct();
        }

        VanBoard? GetBoardByName(string name)
        {
            VanProductionInfo productionInfo;

            if (ProductionVans.ContainsKey(name.ToLower()))
                productionInfo = ProductionVans[name.ToLower()];
            else
            {
                Log.Logger.Error("Van with name {name} does not exist", name.ToLower());
                return null;
            }

            return GetBoardById(productionInfo.Id);
        }
        
        async Task<VanBoard?> GetBoardByNameAsync(string name)
        {
            VanProductionInfo productionInfo;

            if (ProductionVans.ContainsKey(name.ToLower()))
                productionInfo = ProductionVans[name.ToLower()];
            else
            {
                Log.Logger.Error("Van with name {name} does not exist", name.ToLower());
                return null;
            }

            return await GetBoardByIdAsync(productionInfo.Id);
        }

        public IEnumerable<string> ProductionVanIds(bool includeCarPark) => ProductionVanIds(Enum.GetValues<VanModel>(), includeCarPark);

        public IEnumerable<string> ProductionVanIds(IEnumerable<VanModel> vanTypes, bool includeCarPark)
            => ProductionVans.Values.Where(x =>
            x.Handover.HasValue &&
            x.Position is not PreProduction &&
            (includeCarPark || x.Position is not PostProduction) && 
            vanTypes.Contains(x.Name.ToVanType()) && 
            x.HandoverState != HandoverState.HandedOver).Select(x => x.Id);
        
        public IEnumerable<string> GetVanIdsByLocalHandoverDates(params DateTime[] dateTimes)
        {
            IEnumerable<DateTime> formattedDateTimes = dateTimes.Select(x => x.Date);

            return ProductionVans.Values.Where(x =>
                x.Handover.HasValue &&
                formattedDateTimes.Contains(x.Handover.Value.LocalDateTime.Date)
                ).Select(x => x.Id).ToList();
        }

        public IEnumerable<string> GetVanIdsByLocalHandoverDates(DateTime start, DateTime end)
        {
            List<DateTime> dates = new List<DateTime>() { start };
            for (int i = 1; i < (end - start).Days; i++)
            {
                dates.Add(start + TimeSpan.FromDays(i));
            }

            return GetVanIdsByLocalHandoverDates(dates.ToArray());
        }

        public IEnumerable<string> GetVanIdsByLocalHandoverDates(DateTime start, int days)
        {
            List<DateTime> dates = new List<DateTime>() { start };
            for (int i = 1; i < days; i++)
            {
                dates.Add(start + TimeSpan.FromDays(i));
            }

            return GetVanIdsByLocalHandoverDates(dates.ToArray());
        }
        
        public IEnumerable<string> GetVanIdsByLocalHandoverDates(int days, DateTime end)
        {
            List<DateTime> dates = new List<DateTime>() { end };
            for (int i = 0; i < days; i++)
            {
                dates.Add(end - TimeSpan.FromDays(i));
            }

            return GetVanIdsByLocalHandoverDates(dates.ToArray());
        }

        public IEnumerable<string> GetNextExpoHanoverIds(int limit)
            => GetNextHandoverIds(Enum.GetValues<VanModel>().Where(x => !x.IsGen2()), limit);
        
        public IEnumerable<string> GetNextGen2HanoverIds(int limit)
            => GetNextHandoverIds(Enum.GetValues<VanModel>().Where(x => x.IsGen2()), limit);

        public IEnumerable<string> GetNextHandoverIds(IEnumerable<VanModel> vanTypes, int limit)
            => GetNextHandoverIds(vanTypes, DateTimeOffset.Now, TimeSpan.FromDays(7), limit);

        public IEnumerable<string> GetNextHandoverIds(IEnumerable<VanModel> vanTypes, DateTimeOffset start, TimeSpan overdueTimeCutOff, int limit)
        {
            List<string> ids;

            ids = ProductionVans.Values
                 .Where(x => vanTypes.Contains(x.VanModel) && x.Handover.HasValue && start - x.Handover.Value < overdueTimeCutOff && x.HandoverState != HandoverState.HandedOver)
                 .OrderBy(x => x.Handover)
                 .Select(x => x.Id)
                 .Take(limit).ToList();

            return ids;
        }

        public IEnumerable<string> GetLastGen2HandoversIds(int limit)
            => GetLastHanoverIds(Enum.GetValues<VanModel>().Where(x => x.IsGen2()), limit);

        public IEnumerable<string> GetLastExpoHandoversIds(int limit)
            => GetLastHanoverIds(Enum.GetValues<VanModel>().Where(x => !x.IsGen2()), limit);

        public IEnumerable<string> GetLastHanoverIds(IEnumerable<VanModel> vanTypes, int limit)
        {
            List<string> ids;

            ids = ProductionVans.Values
                                .Where(x => vanTypes.Contains(x.VanModel) && x.Handover.HasValue &&
                                            x.HandoverState == HandoverState.HandedOver)
                                .OrderByDescending(x => x.Handover)
                                .Select(x => x.Id)
                                .Take(limit).ToList();

            return ids;
        }

        public IEnumerable<string> GetVanIdsInPosition(IProductionPosition position)
            => GetVanIdsInPositions([position]);

        public IEnumerable<string> GetVanIdsInPositions(params IProductionPosition[] positions)
        {
            return ProductionVans.Values
                .Where(pv => positions.Any(p => p.Equals(pv.Position)))
                .Select(pv => pv.Id);
        }
        public IEnumerable<string> GetVanIdsInPositionInDateRange(IProductionPosition position, DateTime start, TimeSpan timeSpan)
            => GetVanIdsInPositionInDateRange(position, new DateTimeOffset(start), new DateTimeOffset(start + timeSpan));

        public IEnumerable<string> GetVanIdsInPositionInDateRange(IProductionPosition position, DateTime start, DateTime end)
            => GetVanIdsInPositionInDateRange(position, new DateTimeOffset(start), new DateTimeOffset(end));

        public IEnumerable<string> GetVanIdsInPositionInDateRange(IProductionPosition position, DateTimeOffset start, DateTimeOffset end)
        {
            return ProductionVans.Values
                .Where(x => x.PositionHistory.ContainsPositionWithinDate(position, start, end))
                .Select(x => x.Id);
        }

        public IEnumerable<string> GetVanIdsLastInPosition(IProductionPosition position, int numberOfIds)
        {
            return ProductionVans.Values
                                 .Where(x => x.PositionHistory.Any(ph => ph.position.Equals(position)))
                                 .OrderByDescending(x => x.PositionHistory.First(ph => ph.position.Equals(position)).date)
                                 .Take(numberOfIds)
                                 .Select(x => x.Id);
        }

        // TODO: Check if is working correctly
        public IEnumerable<string> GetVansInProductionInDateRange(ProductionLine productionLine, DateTimeOffset start, DateTimeOffset end)
        {
            List<string> results = [];
            IEnumerable<IProductionPosition> positions;

            if (productionLine == ProductionLine.Gen2)
                positions = Gen2ProductionPosition.GetAll();

            else
                positions = ExpoProductionPosition.GetAll();

            foreach (IProductionPosition pos in positions)
            {
                results.AddRange(GetVanIdsInPositionInDateRange(pos, start, end));
            }

            return results.Distinct();
        }
        
        public IEnumerable<string> GetVansEnteredProductionInDateRange(ProductionLine productionLine, DateTimeOffset start, DateTimeOffset end)
        {
            List<string> results = [];
            IProductionPosition position;

            if (productionLine == ProductionLine.Gen2)
                position = Gen2ProductionPosition.First;

            else
                position = ExpoProductionPosition.First;

            results.AddRange(GetVanIdsInPositionInDateRange(position, start, end));
            
            return results;
        }
    }
}