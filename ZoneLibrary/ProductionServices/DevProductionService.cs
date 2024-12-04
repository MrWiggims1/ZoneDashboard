using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace ZoneProductionLibrary.ProductionServices
{
    public class DevProductionService : IProductionService
    {
        public Dictionary<string, VanProductionInfo> ProductionVans { get; private set; } = new Dictionary<string, VanProductionInfo>();
        public ConcurrentDictionary<string, TrelloMember> Members { get; } = new ConcurrentDictionary<string, TrelloMember>();

        public List<ProductionDepartment> Gen2ProductionDepartments { get; set; } = new List<ProductionDepartment>()
        {
            new DevProductionDepartment(
                "Chassis",
                new List<CardAreaOfOrigin>() {CardAreaOfOrigin.Chassis },
                new List<(string ListName, CardAreaOfOrigin area)>() { ("CHASSIS MODULE", CardAreaOfOrigin.Chassis) }
                    ),
            new DevProductionDepartment(
                "Cabinetry",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.CabsAssembly, CardAreaOfOrigin.SubAssembly },
                new List<(string ListName, CardAreaOfOrigin area)>() { ("CABs MODULE", CardAreaOfOrigin.SubAssembly), ("CABs MODULE", CardAreaOfOrigin.CabsAssembly) }
                    
        ),
            new DevProductionDepartment(
                "Wall/Roof Mod",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.WallRoofMod },
                new List<(string ListName, CardAreaOfOrigin area)>() {("WALL/ROOF MOD", CardAreaOfOrigin.WallRoofMod) }
            ),
            new DevProductionDepartment(
                "Electrical",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Bay2 },
                new List<(string ListName, CardAreaOfOrigin area)>() {("BAY 2 ELECTRICAL", CardAreaOfOrigin.Bay2), ("BAY 4 SEALING & ELECTRICAL", CardAreaOfOrigin.Bay2) }
            ),
            new DevProductionDepartment(
                "Bay 1",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Bay1 },
                new List<(string ListName, CardAreaOfOrigin area)>() {("BAY 1 FURNITURE INSTALL", CardAreaOfOrigin.Bay1) }
            ),
            new DevProductionDepartment(
            
                "Bay 3",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Bay3, CardAreaOfOrigin.Toolbox },
                new List<(string ListName, CardAreaOfOrigin area)>() { ("BAY 3 WALL/ROOF INSTALL", CardAreaOfOrigin.Bay3) }
            ),
            new DevProductionDepartment(
                "Commissioning",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Commissioning, CardAreaOfOrigin.Upholstery, CardAreaOfOrigin.CabsFinishing, CardAreaOfOrigin.Detailing },
                new List<(string ListName, CardAreaOfOrigin area)>()
                {
                    ("BAY 6 COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.Commissioning),
                    ("Bay 7 - External Commissioning", CardAreaOfOrigin.Commissioning),
                    ("BAY 6 COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.Detailing),
                    ("BAY 6 COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.Upholstery)
                }
            ),
            new DevProductionDepartment(
                "QC",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.QC },
                new List<(string ListName, CardAreaOfOrigin area)>() { ("QA Cabs Gen2 Checklist", CardAreaOfOrigin.QC), ("BAY 4/5/6 QA (REV003)", CardAreaOfOrigin.QC) }
            )
        };

        public List<ProductionDepartment> ExpoProductionDepartments { get; set; } = new List<ProductionDepartment>()
        {
            new DevProductionDepartment(
                "Chassis",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Chassis},
                new List<(string ListName, CardAreaOfOrigin area)>() {("CHASSIS MODULE", CardAreaOfOrigin.Chassis) }
            ),
            new DevProductionDepartment(
                "Cabinetry",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.CabsAssembly, CardAreaOfOrigin.SubAssembly },
                new List<(string ListName, CardAreaOfOrigin area)>() {("CABs MODULE", CardAreaOfOrigin.CabsAssembly) }
            ),
            new DevProductionDepartment(
                "Wall/Roof Mod",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.WallRoofMod },
                new List<(string ListName, CardAreaOfOrigin area)>() { ("ROOF MOD A", CardAreaOfOrigin.WallRoofMod), ("ROOF MOD B", CardAreaOfOrigin.WallRoofMod) }
            ),
            new DevProductionDepartment(
                "Electrical",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Electrical },
                new List<(string ListName, CardAreaOfOrigin area)>()
                {
                    ("EXPO Electrical bay 1", CardAreaOfOrigin.Electrical),
                    ("EXPO Electrical Bay 2", CardAreaOfOrigin.Electrical),
                    ("EXPO Electrical Bay 3", CardAreaOfOrigin.Electrical)
                }
            ),
            new DevProductionDepartment(
                "Main",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Bay2, CardAreaOfOrigin.Bay3, CardAreaOfOrigin.Bay4, CardAreaOfOrigin.Upholstery },
                new List<(string ListName, CardAreaOfOrigin area)>()
                {
                    ("BAY 2", CardAreaOfOrigin.Bay2),
                    ("BAY 3", CardAreaOfOrigin.Bay3),
                    ("BAY 4", CardAreaOfOrigin.Bay4),
                    ("BAY 4", CardAreaOfOrigin.Upholstery)
                }
            ),
            new DevProductionDepartment(
                "Commissioning",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.Commissioning, CardAreaOfOrigin.CabsFinishing, CardAreaOfOrigin.Detailing },
                new List<(string ListName, CardAreaOfOrigin area)>()
                {
                    ("EXPO COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.Commissioning),
                    ("EXPO COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.CabsFinishing),
                    ("EXPO COMMISSIONING REPORT - PTI's", CardAreaOfOrigin.Detailing)
                }
            ),
            new DevProductionDepartment(
                "QC",
                new List<CardAreaOfOrigin>() { CardAreaOfOrigin.QC },
                new List<(string ListName, CardAreaOfOrigin area)>()
                {
                    ("QA Cabs Expo Checklist (REV002)", CardAreaOfOrigin.QC),
                    ("EXPO LINE QC CHECKS", CardAreaOfOrigin.QC)
                }
            )
        };

        public int VanCount => RandomVanCount;

        public int RedCardCount => boards.Sum(x => x.RedCards.Count());

        public int YellowCount => boards.Sum(x => x.YellowCards.Count());

        public int JobCardCount => boards.Sum(x => x.JobCards.Count());
        public int CheckCount => boards.Sum(x => x.JobCards.Sum(y => y.TotalChecks));

        List<VanBoard> boards = new List<VanBoard>();

        readonly int RandomVanCount = 80;
        readonly TimeSpan TimeInCarpark = TimeSpan.FromDays(4);
        int DayOffset = 0;

        public Task InitializeProductionService()
        {
            Members.TryAdd("1", new TrelloMember("1", "user 1", "User 1", "1"));
            Members.TryAdd("2", new TrelloMember("2", "user 2", "User 2", "1"));
            Members.TryAdd("3", new TrelloMember("3", "user 3", "User 3", "1"));
            Members.TryAdd("4", new TrelloMember("4", "user 4", "User 4", "1"));
            Members.TryAdd("5", new TrelloMember("5", "user 5", "User 5", "1"));
            Members.TryAdd("6", new TrelloMember("6", "user 6", "User 6", "1"));
            Members.TryAdd("7", new TrelloMember("7", "user 7", "User 7", "1"));
            Members.TryAdd("8", new TrelloMember("8", "user 8", "User 8", "1"));
            Members.TryAdd("9", new TrelloMember("9", "user 9", "User 9", "1"));
            Members.TryAdd("10", new TrelloMember("10", "user 10", "User 10", "1"));

            Array types = Enum.GetValues(typeof(VanModel));
            Random random = new Random();

            for (int i = 0; i < RandomVanCount; i++)
            {
                var vanType = (VanModel)types.GetValue(random.Next(types.Length))!; // DevProductionService is not that important
                var vanPos = GetPosition(i, vanType);
                string vanName = Enum.GetName(vanType)!.ToLower() + (i + 1).ToString("000");

                List<JobCard> cards = new List<JobCard>();
                List<RedCard> redCards = new List<RedCard>();

                List<(DateTimeOffset, IProductionPosition)> positionHistory = new List<(DateTimeOffset, IProductionPosition)>();
                IProductionPosition startingPos = vanPos;

                var timeDif = TimeSpan.Zero;

                if (vanType.IsGen2() && vanPos is not PreProduction)
                {
                    if (startingPos is PostProduction)
                    {
                        timeDif += TimeSpan.FromHours(12);
                        positionHistory.Add((DateTime.Now - TimeInCarpark, startingPos));

                        startingPos = Gen2ProductionPosition.Last;
                        positionHistory.Add((positionHistory.First().Item1 - timeDif, startingPos));
                    }
                    else
                        positionHistory.Add((DateTime.Now, startingPos));

                    while ((startingPos--).IsInProduction)
                    {
                        timeDif += TimeSpan.FromHours(12);

                        if (startingPos is not PreProduction) 
                            positionHistory.Add((positionHistory.First().Item1 - timeDif, startingPos));
                    }
                }
                else if (vanPos is not PreProduction)
                {
                    if (startingPos is PostProduction)
                    {
                        timeDif += TimeSpan.FromHours(12);
                        positionHistory.Add((DateTime.Now - TimeInCarpark, startingPos));

                        startingPos = ExpoProductionPosition.Last;
                        positionHistory.Add((positionHistory.First().Item1 - timeDif, startingPos));
                    }
                    else
                        positionHistory.Add((DateTime.Now, startingPos));

                    while ((startingPos--).IsInProduction)
                    {
                        timeDif += TimeSpan.FromHours(12);

                        if(startingPos is not PreProduction)
                            positionHistory.Add((positionHistory.First().Item1 - timeDif, startingPos));
                    }
                }

                positionHistory = positionHistory.OrderBy(x => x.Item1).ToList();

                foreach (DevProductionDepartment dep in vanType.IsGen2() ? Gen2ProductionDepartments : ExpoProductionDepartments)
                {
                    foreach (var list in dep.DepartmentLists)
                    {
                        IProductionPosition jobPos = Gen2ProductionPosition.First;

                        if (vanType.IsGen2())
                        {
                            Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.JobListName, list.ListName, out IProductionPosition? gen2JobPos);
                            if (gen2JobPos is not null)
                                jobPos = gen2JobPos;
                        }
                        else
                        {
                            ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.JobListName, list.ListName, out IProductionPosition? expJobPos);
                            if (expJobPos is not null)
                                jobPos = expJobPos;
                        }

                        var hanoverSoon = GetDateTime(i) - DateTime.Now < TimeSpan.FromDays(1.1);

                        cards.AddRange(RandomBoardData.RandomJobCards(i.ToString(), vanName,random.Next(20, 30), jobPos, list.area, list.ListName, hanoverSoon, vanPos < jobPos, positionHistory.FirstOrDefault(x => x.Item2.Equals(jobPos + 1)).Item1, GetDateTime(i)));
                        redCards.AddRange(RandomBoardData.RandomRedcards(i.ToString(), Enum.GetName(vanType) + (i + 1).ToString("000"), random.Next(0, 3), false, GetDateTime(i), Members.Values, list.area));
                    }
                    
                    IProductionPosition? bayleaderCardPos;
                    
                    if (vanType.IsGen2())
                    {
                        Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.JobListName,
                                                                  dep.DepartmentLists.First().ListName, out bayleaderCardPos);
                    }
                    else
                    {
                        ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.JobListName,
                                                                  dep.DepartmentLists.First().ListName, out bayleaderCardPos);
                    }

                    if (bayleaderCardPos is null)
                        throw new ArgumentNullException();
                    
                    cards.Add(RandomBoardData.PippsCard(i.ToString(), vanName, dep.DepartmentLists.First().ListName, bayleaderCardPos, dep.AreaOfOrigins.First()));
                    cards.Add(RandomBoardData.QualitySignOffCard(i.ToString(), vanName, dep.DepartmentLists.First().ListName, bayleaderCardPos, dep.AreaOfOrigins.First()));
                }

                boards.Add(new VanBoard(
                    i.ToString(),
                    vanName, // DevProductionService is not that important
                    GetDateTime(i),
                    vanPos,
                    vanType.IsGen2() ? Gen2ProductionDepartments : ExpoProductionDepartments,
                    cards,
                    redCards,
                    positionHistory));
            }

            foreach (var board in boards)
            {
                ProductionVans.Add(board.Name, 
                    new VanProductionInfo(board.Id,
                    board.Name,
                    "_blank",
                    board.PositionHistory, 
                    [(DateTimeOffset.Now, board.Handover!.Value)], 
                    DateTimeOffset.Now > board.Handover ? HandoverState.HandedOver : HandoverState.UnhandedOver));
            }

            if (!Directory.Exists(IProductionService.FileBasePath + "attachments"))
                Directory.CreateDirectory(IProductionService.FileBasePath + "attachments");

            return Task.CompletedTask;
        }

        IProductionPosition GetPosition(int i, VanModel vanModel)
        {
            i = (i - TimeInCarpark.Days) / 2;

            if (i < 0)
                return new PostProduction();

            if (vanModel.IsGen2())
            {
                if (i < Gen2ProductionPosition.Positions.Count())
                {
                    Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.LineMoveName,
                                                      Gen2ProductionPosition.Positions
                                                                            .ElementAt(Gen2ProductionPosition.Positions
                                                                                .Count() - 1 - i).name, out IProductionPosition? pos);

                    return (pos is null) ? Gen2ProductionPosition.First : pos;
                }
                
                else
                    return new PreProduction();
            }
            else
            {

                if (i < ExpoProductionPosition.Positions.Count())
                {
                    ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.LineMoveName,
                                                              ExpoProductionPosition.Positions
                                                                  .ElementAt(ExpoProductionPosition.Positions
                                                                                 .Count() - 1 - i).name, out IProductionPosition? pos);

                    return (pos is null) ? ExpoProductionPosition.First : pos;
                }

                else
                    return new PreProduction();
            }
        }

        DateTimeOffset GetDateTime(int i)
        {
            DateTimeOffset dateTime = DateTimeOffset.Now + TimeSpan.FromDays(i / 2);

            dateTime = dateTime.AddDays(DayOffset);

            if (dateTime.DayOfWeek == DayOfWeek.Saturday)
            {
                dateTime = dateTime.AddDays(2);
                DayOffset += 2;
            }

            if (dateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                dateTime = dateTime.AddDays(1);
                DayOffset += 1;
            }

            if (i % 2 == 1)
                dateTime = dateTime - dateTime.TimeOfDay + TimeSpan.FromHours(12);
            else
                dateTime = dateTime - dateTime.TimeOfDay + TimeSpan.FromHours(8);

            return dateTime;
        }

        public Task<VanBoard?> GetBoardByIdAsync(string id)
        {
            return Task.FromResult(boards.SingleOrDefault(x => x.Id == id));
        }
        
        public VanBoard? GetBoardById(string id)
        {
            return boards.SingleOrDefault(x => x.Id == id);
        }

        public Task<IEnumerable<VanBoard>> GetBoardsAsync(IProgress<double> _, IEnumerable<string> ids)
        {
            return Task.FromResult(boards.Where(x => ids.Contains(x.Id)));
        }

        public IEnumerable<VanBoard> GetBoards(IEnumerable<string> ids)
            => boards.Where(x => ids.Contains(x.Id));



        

        public Task<(bool boardfound, string vanId)> TrySearchForVanId(string name, TimeSpan? age = null)
        {
            if (boards.Any(x => x.Name == name))
                return Task.FromResult((true, boards.Single(x => x.Name == name).Id));

            else
                return
                    Task.FromResult((false, string.Empty));
        }

        public Task<IEnumerable<VanBoard>> GetVanByLocalHandoverDatesAsync(IProgress<double> _, params DateTime[] dateTimes)
        {
            if (dateTimes == null || dateTimes.Length == 0)
                throw new ArgumentException("At least one date time must be requested.");

            var fomattedDateTimes = dateTimes.Select(x => x.Date);

            return Task.FromResult(boards.Where(x =>
                x.Handover.HasValue &&
                fomattedDateTimes.Contains(x.Handover.Value.LocalDateTime.Date)
                ));
        }




        
        public JobCard? GetJobCard(string id) => boards.SelectMany(x => x.JobCards).FirstOrDefault(x => x.Id == id);
        
        public Task<IEnumerable<JobCard>> GetJobCardsAsync(IProgress<double> _, IEnumerable<string> boardIds) => Task.FromResult(GetJobCards(boardIds));

        public IEnumerable<JobCard> GetJobCards(IEnumerable<string> boardIds)
        {
            List<JobCard> cards = new List<JobCard>();

            foreach (var board in boards.Where(x => boardIds.Contains(x.Id)))
            {
                cards.AddRange(board.Departments.SelectMany(d => d.JobCards));
            }

            return cards;
        }

        public IEnumerable<JobCard> GetJobCards(IEnumerable<VanModel> vanTypes)
        {
            List<JobCard> cards = new List<JobCard>();

            foreach (var board in boards.Where(x => vanTypes.Contains(x.VanModel)))
            {
                cards.AddRange(board.Departments.SelectMany(d => d.JobCards));
            }

            return cards;
        }

        public Check? GetCheck(string id)
        {
            return boards.SelectMany(x => x.JobCards)
                                 .SelectMany(x => x.CheckLists)
                                 .SelectMany(x => x.Checks)
                                 .FirstOrDefault(x => x.Id == id);
        }

        public Checklist? GetChecklist(string id)
        {
            return boards.SelectMany(x => x.JobCards)
                         .SelectMany(x => x.CheckLists)
                         .FirstOrDefault(x => x.Id == id);
        }


        public IEnumerable<RedCard> GetRedCards()
        {
            List<RedCard> cards = new List<RedCard>();

            foreach (var board in boards)
            {
                cards.AddRange(board.RedCards);
            }

            return cards;
        }

        public RedCard? GetRedCard(string id) => boards.SelectMany(x => x.RedCards).FirstOrDefault(x => x.Id == id);
        public YellowCard? GetYellowCard(string id) => boards.SelectMany(x => x.YellowCards).FirstOrDefault(x => x.Id == id);

        public IEnumerable<YellowCard> GetYellowCards()
        {
            List<YellowCard> cards = new List<YellowCard>();

            foreach (var board in boards)
            {
                cards.AddRange(board.YellowCards);
            }

            return cards;
        }

        public IEnumerable<RedCard> GetRedCards(IEnumerable<VanModel> vanTypes)
        {
            List<RedCard> cards = new List<RedCard>();

            foreach (var board in boards.Where(x => vanTypes.Contains(x.VanModel)))
            {
                cards.AddRange(board.RedCards);
            }

            return cards;
        }

        public IEnumerable<YellowCard> GetYellowCards(IEnumerable<VanModel> vanTypes)
        {
            List<YellowCard> cards = new List<YellowCard>();

            foreach (var board in boards.Where(x => vanTypes.Contains(x.VanModel)))
            {
                cards.AddRange(board.YellowCards);
            }

            return cards;
        }
        
        public IEnumerable<RedCard> GetRedCards(IEnumerable<string> boardIds)
        {
            List<RedCard> cards = new List<RedCard>();

            foreach (var board in boards.Where(x => boardIds.ToList().Contains(x.Id)))
            {
                cards.AddRange(board.RedCards);
            }

            return cards;
        }
        
        public IEnumerable<YellowCard> GetYellowCards(IEnumerable<string> boardIds)
        {
            List<YellowCard> cards = new List<YellowCard>();

            foreach (var board in boards.Where(x => boardIds.ToList().Contains(x.Id)))
            {
                cards.AddRange(board.YellowCards);
            }

            return cards;
        }
        
        public Task<IEnumerable<RedCard>> GetRedCardsAsync(IProgress<double> _, IEnumerable<string> boardIds)
        {
            List<RedCard> cards = new List<RedCard>();

            foreach (var board in boards.Where(x => boardIds.ToList().Contains(x.Id)))
            {
                cards.AddRange(board.RedCards);
            }

            return Task.FromResult((IEnumerable<RedCard>)cards);
        }
        
        public Task<IEnumerable<YellowCard>> GetYellowCardsAsync(IProgress<double> _, IEnumerable<string> boardIds)
        {
            List<YellowCard> cards = [];

            foreach (var board in boards.Where(x => boardIds.ToList().Contains(x.Id)))
            {
                cards.AddRange(board.YellowCards);
            }

            return Task.FromResult((IEnumerable<YellowCard>)cards);
        }

        IEnumerable<RedCard> GetRedCards(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds)
        {
            List<RedCard> cards = new List<RedCard>();

            IEnumerable<VanBoard> _boards;

            if (boardIds == null)
                _boards = boards.Where(x => vanTypes.Contains(x.VanModel));

            else
                _boards = boards.Where(x => boardIds.ToList().Contains(x.Id) && vanTypes.Contains(x.VanModel));

            foreach (var board in _boards)
            {
                cards.AddRange(board.RedCards);
            }

            return cards;
        }

        public Dictionary<CardAreaOfOrigin, List<RedCard>> GetRedCardsByAreaOfOrigin(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds)
        {
            Dictionary<CardAreaOfOrigin, List<RedCard>> values = new Dictionary<CardAreaOfOrigin, List<RedCard>>();

            foreach (CardAreaOfOrigin area in Enum.GetValues<CardAreaOfOrigin>())
            {
                values.Add(area, new List<RedCard>());

                values[area].AddRange(GetRedCards(vanTypes, boardIds).Where(x => x.AreaOfOrigin == area));
            }

            return values;
        }

        public SortedDictionary<DateTime, List<RedCard>> GetRedCardsByLocalDate(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds, DateTime startDate, DateTime endDate)
        {
            SortedDictionary<DateTime, List<RedCard>> values = new SortedDictionary<DateTime, List<RedCard>>();

            startDate = startDate.Date;
            endDate = endDate.Date;

            foreach (var redCard in GetRedCards(vanTypes, boardIds))
            {
                if (!redCard.CreationDate.HasValue)
                    continue;

                var redcardDate = redCard.CreationDate.Value.LocalDateTime.Date;

                if (redcardDate < startDate || redcardDate > endDate)
                    continue;

                if (!values.ContainsKey(redcardDate))
                    values.Add(redcardDate, new List<RedCard>());

                values[redcardDate].Add(redCard);
            }

            return values;
        }

        public Dictionary<RedFlagIssue, List<RedCard>> GetRedCardsByRedFlagType(IEnumerable<VanModel> vanTypes, IEnumerable<string>? boardIds)
        {
            Dictionary<RedFlagIssue, List<RedCard>> values = new Dictionary<RedFlagIssue, List<RedCard>>();

            foreach (RedFlagIssue issue in Enum.GetValues<RedFlagIssue>())
            {
                values.Add(issue, new List<RedCard>());

                values[issue].AddRange(GetRedCards(vanTypes, boardIds).Where(x => x.RedFlagIssue == issue));
            }

            return values;
        }

        public async Task DownloadTrelloFileAsync(string url, string path)
        {
            if (File.Exists(IProductionService.FileBasePath + path))
                return;

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var response = await client.SendAsync(req);

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStreamAsync();

                    using (FileStream fs = new FileStream(IProductionService.FileBasePath + path, FileMode.CreateNew))
                    {
                        await content.CopyToAsync(fs);
                    }
                }
            }

            Log.Logger.Information("New trello file:{url} downloaded to {path}", url, (IProductionService.FileBasePath + path));
        }

        internal class DevProductionDepartment(string name, List<CardAreaOfOrigin> areaOfOrigins, List<(string ListName, CardAreaOfOrigin area)> departmentLists) : ProductionDepartment(name, areaOfOrigins)
        {
            internal List<(string ListName, CardAreaOfOrigin area)> DepartmentLists { get; set; } = departmentLists;
        }
    }
}
