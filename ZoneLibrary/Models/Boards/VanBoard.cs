using Newtonsoft.Json;
using System.Collections.Concurrent;
using ZoneProductionLibrary.ProductionServices.Main;

namespace ZoneProductionLibrary.Models.Boards
{
    public class VanBoard : IFilterableBoard
    {
        [JsonIgnore] public VanModel VanModel => Name.ToVanType();
        public string Id { get; }
        public string Url => $"https://trello.com/b/{this.Id}/";
        public string Name { get; private set; }
        public List<Department> Departments { get; } = new List<Department>();
        public List<RedCard> UnallocatedRedCards { get; } = new List<RedCard>();
        public List<YellowCard> UnallocatedYellowCards { get; } = new List<YellowCard>();
        public DateTimeOffset? Handover { get; private set; }

        [JsonIgnore] public IProductionPosition Position => PositionHistory.Count > 0 ? PositionHistory.Last().position : new PreProduction();
        public List<(DateTimeOffset date, IProductionPosition position)> PositionHistory { get; private set; }

        [JsonIgnore] public double CompletionRate => Departments.Average(d => d.CompletionRate);
        [JsonIgnore] public IEnumerable<JobCard> JobCards => Departments.SelectMany(x => x.JobCards);
        [JsonIgnore] public IEnumerable<RedCard> RedCards => Departments.SelectMany(x => x.RedCards).Concat(UnallocatedRedCards);
        [JsonIgnore] public IEnumerable<YellowCard> YellowCards => Departments.SelectMany(x => x.YellowCards).Concat(UnallocatedYellowCards);

        public VanBoard(
            VanBoardObject boardObject,
            IEnumerable<ProductionDepartment> productionDepartments,
            ProductionService productionService,
            List<(DateTimeOffset date, IProductionPosition position)> positionHistory)
        {
            Id = boardObject.Id;
            Name = boardObject.Name;
            Handover = boardObject.Handover;
            PositionHistory = positionHistory;

            List<JobCard> jobCards = new List<JobCard>();

            foreach (var jobId in boardObject.JobCardIds)
            {
                if (productionService._jobCards.TryGetValue(jobId, out var jobCard))
                {
                    List<Comment> comments = new List<Comment>();
                    
                    foreach (var commentId in jobCard.CommentIds)
                    {
                        if(productionService._comments.TryGetValue(commentId, out var comment))
                        {
                            if (productionService.Members.TryGetValue(comment.CreatorMemberId, out var member))
                                comments.Add(new Comment(comment, member));

                            else
                                Log.Logger.Error("Could not find comment with id:{memberId}", comment.CreatorMemberId);

                        }
                        else
                            Log.Logger.Error("Could not find comment with id:{commentId}", commentId);
                    }
                    
                    if (this.VanModel.IsGen2())
                    {
                        if (Gen2ProductionPosition.TryGetGen2Position(ProductionPositionEntryType.JobListName,
                                                                      jobCard.TrelloListName,
                                                                      out IProductionPosition? pos) &&
                            pos is not null)
                        {
                            jobCards.Add(
                                new JobCard(
                                    jobCard,
                                    productionService,
                                    pos, 
                                    comments));
                        }
                    }
                    else
                    {
                        if (ExpoProductionPosition.TryGetExpoPosition(ProductionPositionEntryType.JobListName,
                                                                      jobCard.TrelloListName,
                                                                      out IProductionPosition? pos) &&
                            pos is not null)
                        {
                            jobCards.Add(
                                new JobCard(
                                    jobCard,
                                    productionService,
                                    pos,
                                    comments));
                        }
                    }


                }
            }

            List<RedCard> redCards = new List<RedCard>();

            foreach (var redcardId in boardObject.RedCardIds)
            {
                if (productionService._redCards.TryGetValue(redcardId, out var redCard))
                {
                    List<Comment> comments = new List<Comment>();
                    List<TrelloMember> redCardMembers = new List<TrelloMember>();

                    foreach (var commentId in redCard.CommentIds)
                    {
                        if(productionService._comments.TryGetValue(commentId, out var comment))
                        {
                            if (productionService.Members.TryGetValue(comment.CreatorMemberId, out var member))
                                comments.Add(new Comment(comment, member));

                            else
                                Log.Logger.Error("Could not find comment with id:{memberId}", comment.CreatorMemberId);

                        }
                        else
                            Log.Logger.Error("Could not find comment with id:{commentId}", commentId);
                    }

                    foreach(var memberId in redCard.MemberIds)
                    {
                        if(productionService.Members.TryGetValue(memberId, out var member))
                            redCardMembers.Add(member);

                        else
                            Log.Logger.Error("Could not find member with id:{memberId}", memberId);
                    }

                    redCards.Add(new RedCard(redCard, Name, Handover, redCardMembers, comments));
                }
            }
            
            List<YellowCard> yellowCards = new List<YellowCard>();

            foreach (var yellowCardId in boardObject.YellowCardIds)
            {
                if (productionService._yellowCards.TryGetValue(yellowCardId, out var yellowCard))
                {
                    List<Comment> comments = new List<Comment>();

                    foreach (var commentId in yellowCard.CommentIds)
                    {
                        if(productionService._comments.TryGetValue(commentId, out var comment))
                        {
                            if (productionService.Members.TryGetValue(comment.CreatorMemberId, out var member))
                                comments.Add(new Comment(comment, member));

                            else
                                Log.Logger.Error("Could not find comment with id:{memberId}", comment.CreatorMemberId);

                        }
                        else
                            Log.Logger.Error("Could not find comment with id:{commentId}", commentId);
                    }

                    yellowCards.Add(new YellowCard(yellowCard, Name, comments));
                }
            }

            foreach (ProductionDepartment productionDepartment in productionDepartments)
            {
                var depCards = jobCards.Where(x => productionDepartment.AreaOfOrigins.Contains(x.AreaOfOrigin));

                Departments.Add(
                    new Department(productionDepartment.Name,
                        depCards,
                        redCards,
                        yellowCards,
                        productionDepartment.AreaOfOrigins));
            }
        }

        public VanBoard(string id, string name, DateTimeOffset? handover, IProductionPosition position, IEnumerable<ProductionDepartment> productionDepartments, IEnumerable<JobCard> jobCards, IEnumerable<RedCard> redCards, List<(DateTimeOffset date, IProductionPosition position)> positionHistory)
        {
            Id = id;
            Name = name;
            Handover = handover;
            PositionHistory = positionHistory;

            foreach (ProductionDepartment productionDepartment in productionDepartments)
            {
                var depCards = jobCards.Where(x => productionDepartment.AreaOfOrigins.Contains(x.AreaOfOrigin));
                
                Departments.Add(
                    new Department(productionDepartment.Name,
                        depCards,
                        redCards,
                        [],
                        productionDepartment.AreaOfOrigins));
            }
        }
    }
}
