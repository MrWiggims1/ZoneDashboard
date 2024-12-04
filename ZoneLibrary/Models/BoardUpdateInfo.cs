using Newtonsoft.Json;

namespace ZoneProductionLibrary.Models
{
    public class BoardUpdateInfo
    {
        public string BoardId { get; } 
        public string? CardId { get; }
        public BoardUpdateType Type { get; } 
        public bool TotalBoardUpdate { get; }

        public override string ToString() => JsonConvert.SerializeObject(this);

        public BoardUpdateInfo(string boardId, BoardUpdateType type, string? cardId = null)
        {
            BoardId = boardId; 
            CardId = cardId;
            Type = type;
            TotalBoardUpdate = false;
        }
    
        public BoardUpdateInfo(string boardId)
        {
            BoardId = boardId;
            CardId = null;
            TotalBoardUpdate = true;
        }
    }
    
    

    public enum BoardUpdateType
    {
        JobCard,
        RedCard,
        YellowCard,
        Hanover,
        Position
    }
}