﻿using System.Drawing;

namespace ZoneProductionLibrary.Models.Boards
{
    public class Department
    {
        public string Name { get; }
        public List<JobCard> JobCards { get; } = new List<JobCard>();
        public List<JobCard> PippsCards => JobCards.Where(x => x.Name == "Team Leader PIPP").ToList();
        public List<JobCard> BayLeaderSignOfCards => JobCards.Where(x => x.Name == "Bay-Leader -- Quality Sign-Off").ToList();
        public List<RedCard> RedCards { get; private set; } = new List<RedCard>();
        public List<CardAreaOfOrigin> RedcardsResponsibleFor { get; } = new List<CardAreaOfOrigin>();


        public double RedcardCompletionRate => RedCards.Count == 0 ? 1d : RedCards.Count(x => x.CardStatus == CardStatus.Completed) / (double)RedCards.Count;
        public Color RedcardColor => TrelloUtil.GetIndicatorColor(RedcardCompletionRate, TargetStatus.Finished);
        public double CompletionRate => GetCompletionRate();
        public double TargetCompletionRate(IProductionPosition vanPosition) => GetTargetCompletionRate(vanPosition);
        public Color Color(IProductionPosition vanPosition) => TrelloUtil.GetIndicatorColor(GetTargetCompletionRate(vanPosition));

        public override string ToString() => Name;

        internal Department(string name, IEnumerable<JobCard> jobCards, IEnumerable<RedCard> redCards, IEnumerable<CardAreaOfOrigin> redcardsResponisbleFor)
        {
            Name = name;
            JobCards = jobCards.ToList();
            RedCards = redCards.Where(x => redcardsResponisbleFor.Contains(x.AreaOfOrigin)).ToList(); ;
            RedcardsResponsibleFor = redcardsResponisbleFor.ToList();
        }

        public List<(string TrelloListName, IEnumerable<JobCard> Cards)> CardsGroupByListName()
        {
            var cardsGroupByListName = JobCards.GroupBy(x => x.TrelloListName, x => x, (key, group) => new { TrelloListName = key, Cards = group });

            List<(string, IEnumerable<JobCard>)> values = new List<(string, IEnumerable<JobCard>)>();

            foreach (var group in cardsGroupByListName)
            {
                values.Add(new(group.TrelloListName, group.Cards));
            }

            return values;
        }

        public void AddRedcards(IEnumerable<RedCard> redcards)
        {
            RedCards.AddRange(redcards);
        }

        public void AddRedcard(RedCard redcard)
        {
            RedCards.Add(redcard);
        }

        private double GetCompletionRate()
        {
            double totalScore = JobCards.Sum(x => x.CompletionRate) + RedCards.Count(x => x.CardStatus == CardStatus.Completed);

            double total = JobCards.Count + RedCards.Count;

            if (total == 0d)
                return 0d;

            return totalScore / total;
        }

        private double GetTargetCompletionRate(IProductionPosition vanPosition)
        {
            var cards = JobCards.Where(x => x.GetTargetStatus(vanPosition) != TargetStatus.NotStarted);
            double totalScore = cards.Sum(x => x.CompletionRate) + RedCards.Count(x => x.CardStatus == CardStatus.Completed);

            double total = cards.Count() + RedCards.Count;

            if (total == 0d)
                return 0d;

            return totalScore / total;
        }
    }
}
