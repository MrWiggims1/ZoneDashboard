namespace ZoneProductionLibrary.Models.ProductionPosition;

public class PreProduction : IProductionPosition
{
    public bool IsInProduction => false;

    public bool IsInCarPark(HandoverState _) => false;

    public string PositionName => "Pre-Production";
    public override string ToString() => PositionName;

    public bool Equals(IProductionPosition? other) => other is PreProduction;

    public bool Equals(IProductionPosition? first, IProductionPosition? second) => first != null && first.Equals(second);

    public int CompareTo(IProductionPosition? other)
    {
        PositionComparer comparer = new PositionComparer();

        return comparer.Compare(this, other);
    }

    public int GetHashCode(IProductionPosition pos)
    {
        if(pos is PreProduction pre)
            return HashCode.Combine(pre, nameof(PreProduction));

        throw new ArgumentException("Position is not pre production");
    }
}