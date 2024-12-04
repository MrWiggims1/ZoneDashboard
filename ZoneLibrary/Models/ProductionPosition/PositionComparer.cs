namespace ZoneProductionLibrary.Models.ProductionPosition;

public class PositionComparer : IComparer<IProductionPosition>
{
	public int Compare(IProductionPosition? x, IProductionPosition? y)
	{
		if (x is null) throw new ArgumentNullException(nameof(x), "Cannot compare null production positions");

		if (y is null) throw new ArgumentNullException(nameof(y), "Cannot compare null production positions");

		if (x is PreProduction)
		{
			if (y is PreProduction)
				return 0;

			return -1;
		}

		if (x is PostProduction)
		{
			if (y is PostProduction)
				return 0;

			return 1;
		}

		if (x.Equals(y)) return 0;

		if (x > y)
			return 1;

		return -1;
	}
}