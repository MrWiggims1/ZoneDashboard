namespace ZoneProductionLibrary.Models
{
    public class VanPositionComparer : IComparer<VanBoard>
    {
        public int Compare(VanBoard? x, VanBoard? y)
        {
            if (x is null || y is null)
                throw new ArgumentNullException(nameof(x), "Cannot compare null vans");

            if (x.Position is PreProduction)
            {
                if (y.Position is PreProduction)
                    return 0;

                else
                    return -1;
            }

            else if (x.Position is PostProduction)
            {
                if (y.Position is PostProduction)
                    return 0;

                else
                    return 1;
            }

            else
            {
                if (x.Position.Equals(y.Position))
                {
                    if (x.PositionHistory.FirstOrDefault(xp => xp.position.Equals(x.Position)).date >
                        x.PositionHistory.FirstOrDefault(yp => yp.position.Equals(y.Position)).date)
                        return -1;

                    else
                        return 1;
                }

                if (x.Position > y.Position)
                    return 1;

                else
                    return -1;
            }
        }
    }
}