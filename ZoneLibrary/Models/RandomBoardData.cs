namespace ZoneProductionLibrary.Models
{
    
    internal static class RandomBoardData
    {
        static int CardId = 0;

        public static JobCard PippsCard(string boardId, string boardName, string trelloListName, IProductionPosition position, CardAreaOfOrigin areaOfOrigin)
        {
            Random random = new Random();
            Checklist checkList =
                new Checklist(
                    "Pipps",
                    new[]
                    {
                        new Check("Item 1", random.Next(2) == 1), 
                        new Check("Item 2", random.Next(2) == 1),
                        new Check("Item 3", random.Next(2) == 1)
                    });

            return new JobCard(CardId.ToString(), 
                               boardId,
                               boardName,
                               "Team Leader PIPP", 
                               trelloListName, 
                               new []{checkList}, 
                               [], 
                               areaOfOrigin,
                               position,
                               checkList.Checks.All(x => x.IsChecked) ? CardStatus.Completed : CardStatus.InProgress,
                               null, TimeSpan.Zero);
        }
        
        public static JobCard QualitySignOffCard(string boardId, string boardName, string trelloListName, IProductionPosition position, CardAreaOfOrigin areaOfOrigin)
        {
            Random random = new Random();
            Checklist checkList =
                new Checklist(
                    "Pipps",
                    new[]
                    {
                        new Check("Item 1", random.Next(2) == 1), 
                        new Check("Item 2", random.Next(2) == 1),
                        new Check("Item 3", random.Next(2) == 1)
                    });

            return new JobCard(CardId.ToString(), 
                               boardId, 
                               boardName,
                               "Bay-Leader -- Quality Sign-Off", 
                               trelloListName, 
                               new []{checkList},
                               [], 
                               areaOfOrigin,
                               position,
                               checkList.Checks.All(x => x.IsChecked) ? CardStatus.Completed : CardStatus.InProgress,
                               null, TimeSpan.Zero);
        }

        private static JobCard RandomCard(string boardId, string boardName, IProductionPosition position, CardAreaOfOrigin areaOfOrigin, string trelloListName, bool forceComplete, bool forceNotStarted, DateTimeOffset started, DateTimeOffset handover, int cardInList)
        {
            Random Random = new Random();
            List<Boards.Checklist> checklists = new List<Boards.Checklist>();
            int checkListCount = Random.Next(0, 3);

            for (int i = 0; i < checkListCount; i++)
            {
                Random = new Random();
                int checkCount = Random.Next(2, 6);
                List<Check> checks = new List<Check>();

                for (int j = 0; j < checkCount; j++)
                {
                    bool isChecked;

                    if (forceComplete)
                        isChecked = true;
                    else if (forceNotStarted)
                        isChecked = false;
                    else
                        isChecked = Random.Next(6) != 0;

                    checks.Add(new Check($"Check **{j + 1}**", isChecked));
                }

                checklists.Add(new Boards.Checklist($"Checklist **{i + 1}**", checks));
            }

            bool isComplete;
            CardStatus status;
            DateTimeOffset? CardStatusLastUpdated = null;

            if (checkListCount == 0)
            {
                if (forceNotStarted)
                    isComplete = false;

                else if (forceComplete)
                    isComplete = true;

                else
                    isComplete = Random.Next(4) != 0;
            }
            else
                isComplete = checklists.All(list => list.Checks.All(checks => checks.IsChecked));

            if (isComplete)
                status = CardStatus.Completed;

            else if (checkListCount == 0 || checklists.All(list => list.Checks.All(checks => !checks.IsChecked)))
                status = CardStatus.NotStarted;

            else if (Random.Next(2) == 0)
                status = CardStatus.InProgress;

            else
                status = CardStatus.UnableToComplete;

            if (status != CardStatus.NotStarted && status != CardStatus.Unknown)
            {
                var timeToHandover = handover - DateTimeOffset.Now;

                if (Random.Next(4) == 0)
                    CardStatusLastUpdated = started + ((handover - started - timeToHandover) * Random.NextDouble());

                else if (timeToHandover < TimeSpan.FromHours(24))
                {
                     if (Random.Next(3) == 0)
                        CardStatusLastUpdated = handover - timeToHandover - TimeSpan.FromHours(Random.NextDouble() * 12);

                    else
                        CardStatusLastUpdated = started + TimeSpan.FromHours(Random.NextDouble() * 12);
                }
                else
                    CardStatusLastUpdated = started + TimeSpan.FromHours(Random.NextDouble() * 12);
            }

            TimeSpan taskTime;

            if (Random.Next(5) == 0)
                taskTime = TimeSpan.Zero;
            else
                taskTime = TimeSpan.FromMinutes(Random.Next(10, 90));

            JobCard card = new JobCard(CardId.ToString(), boardId, boardName, $"{Enum.GetName(areaOfOrigin)} {cardInList}", trelloListName, checklists, [], areaOfOrigin, position, status, CardStatusLastUpdated, taskTime);

            CardId++;

            return card;
        }

        internal static List<JobCard> RandomJobCards(string boardId, string boardName,int cardCount, IProductionPosition position, CardAreaOfOrigin areaOfOrigin, string trelloListName, bool forceComplete, bool forceNotStarted, DateTimeOffset jobStarted, DateTimeOffset handover)
        {
            List<JobCard> cards = new List<JobCard>();

            for (int i = 0; i < cardCount; i++)
            {
                cards.Add(RandomCard(boardId, boardName, position, areaOfOrigin, trelloListName, forceComplete, forceNotStarted, jobStarted, handover, i));
                CardId++;
            }

            return cards;
        }

        internal static List<RedCard> RandomRedcards(string boardId, string boardName, int cardCount, bool forceComplete, DateTimeOffset handoverDate, IEnumerable<TrelloMember> members, params CardAreaOfOrigin[] areasOfOrigin)
        {
            List<RedCard> cards = new List<RedCard>();

            for (int i = 0; i < cardCount; i++)
            {
                Random random = new Random();

                int option = random.Next(0, RedcardOptions.Count());
                CardStatus status = forceComplete ? CardStatus.Completed : (random.Next(2) == 0 ? CardStatus.Completed : CardStatus.NotStarted);
                CardAreaOfOrigin origin = areasOfOrigin[random.Next(areasOfOrigin.Count())];

                List<TrelloMember> ranMembers = new List<TrelloMember>();

                ranMembers.Add(members.ElementAt(i % members.Count()));
                ranMembers.Add(members.ElementAt((i + 1) % members.Count()));

                List<Comment> comments = new List<Comment>();

                int commentCount = random.Next(0, 3);

                DateTimeOffset creationDate = DateTimeOffset.Now - TimeSpan.FromHours(random.NextDouble() * 24 * 30);

                if (creationDate.LocalDateTime.DayOfWeek == DayOfWeek.Sunday)
                    creationDate += TimeSpan.FromDays(1);

                else if (creationDate.LocalDateTime.DayOfWeek == DayOfWeek.Saturday)
                    creationDate += TimeSpan.FromDays(2);

                DateTimeOffset? CardStatusLastUpdated = null;
                if (status == CardStatus.Completed)
                {
                    TimeSpan timeRange = DateTimeOffset.Now - creationDate;
                    CardStatusLastUpdated = creationDate + TimeSpan.FromHours(random.NextDouble() * timeRange.TotalHours);
                }

                for (int j = 0; j < commentCount; j++)
                {
                    comments.Add(new Comment(ranMembers[0], creationDate.LocalDateTime + TimeSpan.FromMinutes((j * 46) + random.Next(5, 50)), $"Comment {j} has lots of text __aaaaaaaa aaaaaaaaaa aaaa__ aaaaaa aaaaaaaaaaaa aaaaaaa aaaaaaa **bold text AAAHHHHH**"));
                }

                cards.Add(new RedCard(CardId.ToString(), boardId, boardName, RedcardOptions[option].name, RedcardOptions[option].issue, status, origin, handoverDate, creationDate, ranMembers, comments, CardStatusLastUpdated));
                CardId++;
            }

            return cards;
        }

        static private (string name, RedFlagIssue issue)[] RedcardOptions =
        {
            ("Thingy missing", RedFlagIssue.MissingPart),
            ("Thingy Broken", RedFlagIssue.Damage),
            ("Annother Thingy missing", RedFlagIssue.MissingPart),
            ("Out of stock", RedFlagIssue.OutOfStock),
            ("Needs redesign", RedFlagIssue.DesignIssue),
            ("Need new process", RedFlagIssue.BuildProcess),
            ("Super long titled card because displaying these is hard and annoying to get right. Extra words just to make the name longer, no need to read. Dont mind the name its still going", RedFlagIssue.WorkmanShip)
        };
    }
}