using DBLibrary.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ZoneProductionLibrary.Models.UpdateData;

namespace ZoneProductionLibrary.ProductionServices.Main
{
    public interface IProductionService
    {
        public static event EventHandler<BoardUpdateInfo>? BoardUpdated;
        public static event EventHandler<string>? VanAddedToProduction;
        
        public ConcurrentDictionary<string, VanProductionInfo> ProductionVans { get; }
        public ConcurrentDictionary<string, Employee> Employees { get; }
        public ConcurrentDictionary<string, CheckObject>     Checks { get; }
        public ConcurrentDictionary<string, ChecklistObject> CheckLists  { get; }
        public ConcurrentDictionary<string, JobCardObject>   JobCards    { get; }
        public ConcurrentDictionary<string, RedCardObject>   RedCards    { get; }
        public ConcurrentDictionary<string, RedCardObject>   YellowCards { get; }
        public ConcurrentDictionary<string, VanBoardObject>  VanBoards   { get; }
        public ConcurrentDictionary<string, CommentObject>   Comments    { get; }
        
        Task Initialize();
        
        VanBoard? GetBoard(string id);
        JobCard? GetJobCard(string id);
        RedCard? GetRedCard(string id);
        YellowCard? GetYellowCard(string id);
        
        Task<VanBoard?> GetBoardSingleOrDefaultAsync(Expression<Func<IFilterableBoard, bool>> predicate);
        
        void UpdateCheck(object? sender, CheckUpdatedData e);
        void DeleteCheck(object? sender, CheckDeletedData e);
        void CreateCheck(object? sender, CheckUpdatedData e);
        void DeleteCheckList(object? sender, CheckListDeletedData e);
        void CreateCheckList(object? sender, CheckListCreatedData e);
        void CopyCheckList(object? sender, CheckListCreatedData e);
        void UpdateCheckList(object? sender, CheckListUpdatedData e);
        void CreateCard(object? sender, CardUpdatedData e);
        void DeleteCard(object? sender, CardUpdatedData e);
        void UpdateCard(object? sender, CardUpdatedData e);
        void MemberAddedToCard(object? sender, MemberAddedToCardData e);
        void MemberRemovedFromCard(object? sender, MemberAddedToCardData e);
        void UpdateCustomFieldItems(object? sender, CardUpdatedData e);
        void UpdateCommentsOnCard(object? sender, CardUpdatedData e);
        void AttachmentAdded(object? sender, AttachmentAddedData e);
        void AttachmentDeleted(object? sender, AttachmentRemovedData e);
        void UpdateHandoverInfo(object? sender, CardUpdatedData e);
        void UpdatedLineMoveInfo(object? sender, CardUpdatedData e);
        
        Task<IEnumerable<VanBoard>> GetBoardsAsync(IProgress<double> progress, Expression<Func<IFilterableBoard, bool>> predicate);
        
        IEnumerable<JobCard> GetJobCards(Expression<Func<IFilterableCard, bool>> predicate);
        IEnumerable<RedCard> GetRedCards(Expression<Func<IFilterableCard, bool>> predicate);
        IEnumerable<YellowCard> GetYellowCards(Expression<Func<IFilterableCard, bool>> predicate);
        
        IEnumerable<JobCard> GetBoardJobCards(Expression<Func<IFilterableBoard, bool>> predicate);
        IEnumerable<RedCard> GetBoardRedCards(Expression<Func<IFilterableBoard, bool>> predicate);
        IEnumerable<YellowCard> GetBoardYellowCards(Expression<Func<IFilterableBoard, bool>> predicate);
        
        Task<IEnumerable<RedCard>> GetBoardRedCardsAsync(IProgress<double> progress, Expression<Func<IFilterableCard, bool>> predicate);
        Task<IEnumerable<RedCard>> GetBoardJobCardsAsync(IProgress<double> progress, Expression<Func<IFilterableCard, bool>> predicate);
        Task<IEnumerable<YellowCard>> GetBoardYellowCardsAsync(IProgress<double> progress, Expression<Func<IFilterableCard, bool>> predicate);
        
        VanBoardObject? GetBoardObject(string boardId);
        IEnumerable<CheckObject> GetBoardCheckObjects(string boardId);
        IEnumerable<ChecklistObject> GetBoardCheckListObjects(string boardId);
        IEnumerable<JobCardObject> GetBoardJobCardObjects(string boardId);
        IEnumerable<RedCardObject> GetBoardRedCardObjects(string boardId);
        
        

        public IEnumerable<Employee> GetEmployees(Expression<Func<Employee, bool>> predicate)
            => this.Employees.Values.Where(predicate.Compile());
        
        public IEnumerable<Comment> GetComments(Expression<Func<CommentObject, bool>> predicate)
            => this.Comments.Values.Where(predicate.Compile()).Select(GetCommentFromObject);
    
        private Comment GetCommentFromObject(CommentObject co)
        {
            if (Employees.TryGetValue(co.CreatorId, out var employee))
            {
                return new Comment(co, employee);
            }
            else
            {
                throw new KeyNotFoundException($"Could not find employee id {co.CreatorId}");
            }
        }
    }
}