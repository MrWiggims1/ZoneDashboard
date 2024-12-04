using AngleSharp.Text;
using DBLibrary.Models;
using Serilog.Context;
using TrelloDotNet.Model;
using ZoneProductionLibrary.ProductionServices.Main;

namespace ZoneProductionLibrary.Models.BoardObjects
{
    public class VanBoardObject : IEqualityComparer<VanBoardObject>, IEquatable<VanBoardObject>
    {
        public string Id { get; }
        public string Name { get; }
        public List<string> JobCardIds { get; } = [];
        public List<string> RedCardIds { get; } = [];
        public List<string> YellowCardIds { get; } = [];
        public DateTimeOffset? Handover { get; internal set; }

        public override string ToString() => $"Van {Id}: {Name}";

        public static CardType GetCardType(string cardName, string listName)
        {
            if (RedCardListNames.Contains(listName) && !IgnoredRedAndYellowCardNames.Contains(cardName))
                return CardType.RedCard;

            if (YellowCardListNames.Contains(listName) && !IgnoredRedAndYellowCardNames.Contains(cardName))
                return CardType.YellowCard;

            if (!IgnoredJobListsNames.Contains(cardName) && !RedCardListNames.Contains(listName) &&
                !YellowCardListNames.Contains(listName))
                return CardType.JobCard;

            return CardType.None;
        }

        public static readonly string[] RedCardListNames =
            ["RED CARDS TO BE ACTIONED", "RED FLAG CARDS COMPLETED", "RED FLAG CARDS COMPLETEDI\u2076", "Design Issues"];

        public static readonly string[] YellowCardListNames =
            ["Yellow Cards___________ (Due to out of stock parts!)"];

        public static readonly string[] IgnoredRedAndYellowCardNames =
        [
            "IMPORTANT STEP's ON - How to raise a red card",
            "STEPS TO CLOSE OUT A RED CARD WHEN COMPLETED",
            "DAMAGE & WORKMANSHIP >>>>>ONLY<<<<<",
            "COMPLETED YELLOW & RED-CARDs",
            "HOW TO Create a Yellow Card",
            "Part Number/Description.",
            ">>RED CARDS COMPLETED<<"
        ];

        public static readonly string[] IgnoredJobListsNames =
        [
            "PLANS AND SPECS",
            "COMPLIANCE CERTIFICATES",
            "COMPLIANCE    & CERTIFICATES",
            "FINAL SIGN-OFFs___G2",
            "SIGN-OFFs",
            "PICKING PICTURES",
            "STORES - LATE PARTS COMMS",
            "VAN PROGRESS PHOTOS",
            "HANDOVER Day",
            "QC CHECKS",
            "HangarO",
            "DETAILING",
            "Yellow Cards___________ (Due to out of stock parts!)",
            "WELDING",
            "Van Board Creation _____(remove list once done)",
            "PLANS AND SPECS",
            "SIGN-OFFs"
        ];

        public VanBoardObject(
            ProductionService productionService,
            string boardId,
            string boardName,
            IEnumerable<Card> cards,
            List<CustomField> customFields,
            List<CachedTrelloAction> actions,
            DateTimeOffset? handover)
        {
            this.Id = boardId;
            this.Name = boardName;
            this.Handover = handover;

            foreach (Card card in cards)
            {
                using (LogContext.PushProperty("CardLink", $"https://trello.com/c/{card.Id}"))
                using (LogContext.PushProperty("CardName", card.Name))
                {
                    List<CachedTrelloAction> cardActions = actions.Where(x => x.CardId == card.Id).ToList();

                    CardType cardType = GetCardType(card.Name, card.List.Name);

                    if (cardType == CardType.RedCard)
                    {
                        IEnumerable<string> commentIds =
                            AddComments(productionService, cardActions.Where(x => x.ActionType == "commentCard").ToList());

                        string? cardCreatorId = actions
                                                .SingleOrDefault(
                                                    x => x.CardId == card.Id && x.ActionType == "createCard")
                                                ?.MemberId;

                        RedCardObject newCard = new RedCardObject(card,
                                                                  customFields,
                                                                  cardActions,
                                                                  commentIds,
                                                                  cardCreatorId);

                        productionService._redCards.TryAdd(card.Id, newCard);

                        this.RedCardIds.Add(card.Id);
                    }
                    else if (cardType == CardType.YellowCard)
                    {
                        IEnumerable<string> commentIds =
                            AddComments(productionService, cardActions.Where(x => x.ActionType == "commentCard").ToList());

                        string? cardCreatorId = actions
                                                .SingleOrDefault(
                                                    x => x.CardId == card.Id && x.ActionType == "createCard")
                                                ?.MemberId;

                        RedCardObject newCard = new RedCardObject(card,
                                                                  customFields,
                                                                  cardActions,
                                                                  commentIds,
                                                                  cardCreatorId);

                        productionService._yellowCards.TryAdd(card.Id, newCard);

                        this.YellowCardIds.Add(card.Id);
                    }
                    else if (cardType == CardType.JobCard &&
                             TrelloUtil.ToCardAreaOfOrigin(card, customFields) != CardAreaOfOrigin.Unknown)
                    {
                        IEnumerable<string> commentIds =
                            AddComments(productionService, cardActions.Where(x => x.ActionType == "commentCard").ToList());

                        JobCardObject newCard = new JobCardObject(productionService,
                                                                  card,
                                                                  customFields,
                                                                  cardActions,
                                                                  commentIds);

                        productionService._jobCards.TryAdd(card.Id, newCard);

                        this.JobCardIds.Add(card.Id);
                    }
                }
            }
        }

        public static IEnumerable<string> AddComments(
            ProductionService productionService, List<CachedTrelloAction> commentActions)
        {
            List<string> comments = [];

            foreach (CachedTrelloAction action in commentActions)
            {
                CommentObject comment = new CommentObject(action);

                productionService._comments.TryAdd(comment.Id, comment);
                comments.Add(comment.Id);
            }

            return comments;
        }

        public bool Equals(VanBoardObject? other) => Equals(this, other);

        public bool Equals(VanBoardObject? x, VanBoardObject? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            if (x.GetType() != y.GetType())
                return false;

            return x.Id == y.Id && x.Name == y.Name && x.JobCardIds.Equals(y.JobCardIds) &&
                   x.RedCardIds.Equals(y.RedCardIds) && x.YellowCardIds.Equals(y.YellowCardIds) &&
                   Nullable.Equals(x.Handover, y.Handover);
        }

        public CompareReport? Compare(VanBoardObject? other)
        {
            CompareReport report = new CompareReport(ToString());
            
            if (ReferenceEquals(this, other))
            {
                return null;
            }

            if (ReferenceEquals(this, null))
            {
                report.Issues[ToString()].Add("This null Reference");
                report.Pass = false;
                
                return report;
            }

            if (ReferenceEquals(other, null)) 
            {
                report.Issues[ToString()].Add("other null Reference");
                report.Pass = false;
                
                return report;
            }

            if (this.Id != other.Id)
            {
                report.Issues[ToString()].Add($"Id: {this.Id} != {other.Id}");
                report.Pass = false;
            }
            if (this.Name != other.Name)
            {
                report.Issues[ToString()].Add($"Name: {this.Name} != {other.Name}");
                report.Pass = false;
            }
            if (!Nullable.Equals(this.Handover, other.Handover))
            {
                report.Issues[ToString()].Add($"Handover: {this.Handover.ToString()} != {other.Handover.ToString()}");
                report.Pass = false;
            }
            
            List<string> otherJobIds = new List<string>(other.JobCardIds);
            
            foreach (string jobId in this.JobCardIds)
            {
                if (otherJobIds.Contains(jobId))
                {
                    otherJobIds.Remove(jobId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Job card Id {jobId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherJobIds)
            {
                report.Issues[ToString()].Add($"Job card Id {missedId} missing from this");
                report.Pass = false;
            }
            
            List<string> otherRedIds = new List<string>(other.RedCardIds);
            
            foreach (string redId in this.RedCardIds)
            {
                if (otherRedIds.Contains(redId))
                {
                    otherRedIds.Remove(redId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Red card Id {redId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherRedIds)
            {
                report.Issues[ToString()].Add($"Red card Id {missedId} missing from this");
                report.Pass = false;
            }
            
            List<string> otherYellowIds = new List<string>(other.YellowCardIds);
            
            foreach (string yellowId in this.YellowCardIds)
            {
                if (otherYellowIds.Contains(yellowId))
                {
                    otherYellowIds.Remove(yellowId);
                }
                else
                {
                    report.Issues[ToString()].Add($"Yellow card Id {yellowId} missing from other");
                    report.Pass = false;
                }
            }

            foreach (string missedId in otherYellowIds)
            {
                report.Issues[ToString()].Add($"Yellow card Id {missedId} missing from this");
                report.Pass = false;
            }

            if (report.Pass == false)
                return report;

            else
                return null;
        }

        public int GetHashCode(VanBoardObject obj) => HashCode.Combine(obj.Id,
                                                                       obj.Name,
                                                                       obj.JobCardIds,
                                                                       obj.RedCardIds,
                                                                       obj.YellowCardIds,
                                                                       obj.Handover);
    }
}