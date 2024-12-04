using DBLibrary.Data;
using DBLibrary.Models;
using Serilog.Context;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using TrelloDotNet;
using TrelloDotNet.Model.Webhook;
using ZoneProductionLibrary.ProductionServices.Main;

namespace ZoneLibrary.Services.TrelloProduction
{
    public partial class TrelloProductionService : IProductionService
    {
        private IVanIdData _vanIdDataDB;
        private ITrelloActionData _trelloActionDataDB;
        private TrelloClient _trelloClient = null!;

        private List<Webhook> _webhooks = [];

        public static readonly string CCDashboardId   = "5f1a1f029e25dd741ebc3466";
        public static readonly string LineMoveBoardId = "6089f58c359e19533e9b7f1c";

        public ConcurrentDictionary<string, VanProductionInfo> ProductionVans { get; } = [];
        public ConcurrentDictionary<string, Employee>          Employees { get; }   = [];
        public ConcurrentDictionary<string, CheckObject>       Checks  { get; }     = [];
        public ConcurrentDictionary<string, ChecklistObject>   CheckLists { get; }  = [];
        public ConcurrentDictionary<string, JobCardObject>     JobCards { get; }    = [];
        public ConcurrentDictionary<string, RedCardObject>     RedCards { get; }    = [];
        public ConcurrentDictionary<string, RedCardObject>     YellowCards { get; } = [];
        public ConcurrentDictionary<string, VanBoardObject>    VanBoards { get; }   = [];
        public ConcurrentDictionary<string, CommentObject>     Comments { get; }    = [];
        
        
        public TrelloProductionService(IVanIdData vanIdData, ITrelloActionData trelloActionData)
        {
            _vanIdDataDB = vanIdData;
            _trelloActionDataDB = trelloActionData;
        }
    }
}