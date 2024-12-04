namespace ZoneProductionLibrary.Models.ProductionPosition;

public interface IProductionPosition : IEquatable<IProductionPosition>, IEqualityComparer<IProductionPosition>, IComparable<IProductionPosition>
{
    public bool IsInProduction { get; }
    public bool IsInCarPark(HandoverState handoverState);
    public string PositionName { get; }
    public string ShortPositionName
    {
        get
        {
            return PositionName;
        }
    }
    public static bool operator <(IProductionPosition first, IProductionPosition second)
    {
        if (first is PreProduction) 
            return second is not PreProduction;

        if (first is PostProduction) 
            return false;

        if (first is Gen2ProductionPosition gen1)
        {
            if (second is PreProduction)
                return false;

            if (second is PostProduction)
                return true;

            if (second is Gen2ProductionPosition gen2)
                return gen1.PositionId < gen2.PositionId;

            return true;
        }
        
        if (first is ExpoProductionPosition exp1)
        {
            if (second is PreProduction)
                return false;

            if (second is PostProduction)
                return true;

            if (second is ExpoProductionPosition exp2)
                return exp1.PositionId < exp2.PositionId;

            return false;
        }

        throw new Exception("unhandled case in comparing IProductionPosition occured");
    }

    public static bool operator >(IProductionPosition first, IProductionPosition second)
    {
        if (first is PreProduction) 
            return false;

        if (first is PostProduction) 
            return second is not PostProduction;
        
        if (first is Gen2ProductionPosition gen1)
        {
            if (second is PreProduction)
                return true;

            if (second is PostProduction)
                return false;

            if (second is Gen2ProductionPosition gen2)
                return gen1.PositionId > gen2.PositionId;

            return false;
        }
        
        if (first is ExpoProductionPosition exp1)
        {
            if (second is PreProduction)
                return true;

            if (second is PostProduction)
                return false;

            if (second is ExpoProductionPosition exp2)
                return exp1.PositionId > exp2.PositionId;

            return true;
        }

        throw new Exception("unhandled case in comparing IProductionPosition occured");
    }

    public static IProductionPosition operator ++(IProductionPosition position)
    {
        if (position is Gen2ProductionPosition gen2)
            return gen2 + 1;

        if (position is ExpoProductionPosition expo)
            return expo + 1;

        if (position is PostProduction post)
            return post;

        throw new ArgumentException("Cant increment preproduction position");
    }

    public static IProductionPosition operator --(IProductionPosition position)
    {
        if (position is Gen2ProductionPosition gen2)
            return gen2 - 1;

        if (position is ExpoProductionPosition expo)
            return expo - 1;

        if (position is PreProduction pre)
            return pre;

        throw new ArgumentException("Cant decrement post production position");
    }

    public static IProductionPosition operator +(IProductionPosition position, int add)
    {
        if (position is Gen2ProductionPosition gen2)
            return gen2 + add;

        if (position is ExpoProductionPosition expo)
            return expo + add;

        if (position is PostProduction post)
            return post;

        throw new ArgumentException("Preproduction cannot be added");
    }

    public static IProductionPosition operator -(IProductionPosition position, int subtract)
    {
        if (position is Gen2ProductionPosition gen2)
            return gen2 - subtract;

        if (position is ExpoProductionPosition expo)
            return expo - subtract;

        if (position is PostProduction)
            throw new ArgumentException("Post production cannot be subtracted");

        return new PreProduction();
    }

    public static List<string> IgnoredNames =
        [
            "Design issues __________________ NO RED CARDs TO BE ADDED TO THIS COLUMN",
            "OUTSIDE - Carpark GEN2",
            "OUTSIDE - Carpark GEN2 (Fishbowl closed)"
        ];
}