namespace ZoneProductionLibrary.Models
{
    public interface IFilterableCard
    {
        public string Name { get; }
        public string BoardId { get; }
        public CardAreaOfOrigin AreaOfOrigin { get; }
        public CardStatus CardStatus { get; }
    }

    public interface IFilterableBoard
    {
        public string Id { get; }
        public VanModel VanModel { get; }
        IProductionPosition Position { get; }
    }
}
