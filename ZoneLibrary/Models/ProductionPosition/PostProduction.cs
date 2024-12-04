namespace ZoneProductionLibrary.Models.ProductionPosition;

public class PostProduction : IProductionPosition
{
	public bool IsInProduction => false;
	public bool IsInCarPark(HandoverState handoverState) => handoverState == HandoverState.UnhandedOver;

	public string PositionName => "Post Production";

    public override string ToString() => PositionName;

	public bool Equals(IProductionPosition? other) => other is PostProduction;

	public bool Equals(IProductionPosition? first, IProductionPosition? second) => first != null && first.Equals(second);

    public int CompareTo(IProductionPosition? other)
    {
        PositionComparer comparer = new PositionComparer();

        return comparer.Compare(this, other);
    }

	public int GetHashCode(IProductionPosition pos)
	{
		if(pos is PostProduction post)
			return HashCode.Combine(post, nameof(PostProduction));

		throw new ArgumentException("Position is not post production");
	}
}