namespace ZoneProductionLibrary.Models.ProductionPosition;

public class ExpoProductionPosition : IProductionPosition
{
    public bool IsInProduction => true;
    public bool IsInCarPark(HandoverState _) => false;

    public int PositionId { get; }
    public string PositionName => Positions.Single(x => x.id == PositionId).name;
    public string ShortPositionName => Positions.Single(x => x.id == PositionId).shortName;

    public override string ToString() => $"{PositionName} {PositionId}";

    public bool Equals(IProductionPosition? other)
    {
        if (other is ExpoProductionPosition expoPos)
            return expoPos.PositionId == PositionId;

        return false;
    }
        
    public bool Equals(IProductionPosition? first, IProductionPosition? second) => first != null && first.Equals(second);

    public int CompareTo(IProductionPosition? other)
    {
        PositionComparer comparer = new PositionComparer();

        return comparer.Compare(this, other);
    }

    public int GetHashCode(IProductionPosition pos)
    {
        if(pos is ExpoProductionPosition expo)
            return HashCode.Combine(nameof(ExpoProductionPosition), expo.PositionId);

        throw new ArgumentException("Position is not expo");
    }
    
    
    public static ExpoProductionPosition First => new ExpoProductionPosition(0);
    public static ExpoProductionPosition Last => new ExpoProductionPosition(Positions.Last().id);

    public static IEnumerable<IProductionPosition> GetAll()
    {
        List<IProductionPosition> results = [];

        foreach (var position in Positions)
        {
            results.Add(new ExpoProductionPosition(position.id));
        }

        return results;
    }

    public static IProductionPosition operator +(ExpoProductionPosition position, int add)
    {
        if (position.PositionId + add > Positions.Last().id) return new PostProduction();

        string newPos = Positions.Single(x => x.id == position.PositionId + add).name;
        return new ExpoProductionPosition(ProductionPositionEntryType.LineMoveName, newPos);
    }

    public static IProductionPosition operator -(ExpoProductionPosition position, int subtract)
    {
        if (position.PositionId - subtract < 0) return new PreProduction();

        string newPos = Positions.Single(x => x.id == position.PositionId - subtract).name;
        return new ExpoProductionPosition(ProductionPositionEntryType.LineMoveName, newPos);
    }
        
    private ExpoProductionPosition(int positionId)
    {
        if(Positions.All(x => x.id != positionId))
            throw new ArgumentOutOfRangeException(nameof(positionId), $"Expo production does not contain a value with id {positionId}");

        PositionId = positionId;
    }

    private ExpoProductionPosition(ProductionPositionEntryType type, string name)
    {
        if (type == ProductionPositionEntryType.LineMoveName)
            this.PositionId = Positions.Single(x => x.name == name).id;

        else
            this.PositionId = BoardPositions.First(x => x.name == name).id;
    }

    public static bool TryGetExpoPosition(ProductionPositionEntryType type, string name, out IProductionPosition? position)
    {
        if (IProductionPosition.IgnoredNames.Contains(name))
        {
            position = null;
            return false;
        }
        
        if (type == ProductionPositionEntryType.LineMoveName)
        {
            if (Positions.Any(x => x.name == name))
            {
                position = new ExpoProductionPosition(type, name);
                return true;
            }
        }

        else if (BoardPositions.Any(x => x.name == name))
        {
            position = new ExpoProductionPosition(type, name);
            return true;
        }
        
        Log.Logger.Debug("{listName} is not a valid Expo list name", name);

        position = null;
        return false;
    }

    public static readonly (string name, string shortName, int id)[] Positions =
    [
        ("Chassis Module Expo", "Chassis", 0),
        ("EXPO BAY 1", "Bay 1", 1),
        ("EXPO BAY 2", "Bay 2", 2),
        ("Outside/Paint Bay", "Paint", 3),
        ("EXPO BAY 3", "Bay 3", 4),
        ("EXPO BAY 4", "Bay 4", 5),
        ("EXPO BAY 5", "Bay 5", 6),
        ("Dome - EXPO", "Dome", 7)
    ];

    public static readonly (string name, int id)[] BoardPositions =
    [
        ("WELDING", 0),
        ("CHASSIS MODULE", 0),
        ("[QC] Chassis Bay", 0),
        ("CABs MODULE", 0),
        ("CABS MODULE", 0),
        ("QA Cabs Expo Checklist", 0),
        ("[QC] Cabs Expo Checklist", 0),
        ("CABS PREP expo", 0),
        ("SUB ASSAMBLY", 0),
        ("SUB ASSAMBLY (REV001)", 0),
        ("SUB ASSAMBLY (REV002)", 0),
        ("SUB ASSAMBLY (REV003)", 0),
        ("QA Cabs Expo Checklist (REV002)", 0),
        ("BAY 1 MODULE INSTALL", 1),
        ("[QC] BAY 1 Expo Checklist", 1),
        ("EXPO Electrical bay 1", 1),
        ("BAY 1 ELECTRICAL", 1),
        ("WALL MOD", 1),
        ("WALL PREP BAY", 1),
        ("[QC] WALL PREP BAY Expo Checklist", 1),
        ("[QC] ROOF BAY Expo Checklist", 1),
        ("[QC] WALL BAY Expo Checklist", 1),
        ("ROOF MOD A", 1),
        ("ROOF MOD", 1),
        ("ROOF MOD B", 1),
        ("ROOF BAY Expo Checklist", 1),
        ("QC WALL BAY Expo Checklist", 1),
        ("MODE - EXPO DOOR MOD", 2),     //TODO: Confirm Position
        ("[QC] MODE Expo Checklist", 2), //TODO: Confirm Position
        ("BAY 2", 2),
        ("[QC] BAY 2 Expo Checklist", 2),
        ("EXPO Electrical Bay 2", 2),
        ("PAINT BAY", 3),
        ("[QC] PAINT Expo Checklist", 3),
        ("QC PAINT Expo Checklist", 3),
        ("BAY 3", 4),
        ("[QC] BAY 3 Expo Checklist", 4),
        ("EXPO Electrical Bay 3", 4),
        ("BAY 4", 5),
        ("[QC] BAY 4 Expo Checklist", 5),
        ("EXPO COMMISSIONING (REV002)", 6),
        ("BAY 5 EXPO COMMISSIONING (REV002)", 6),
        ("EXPO COMMISSIONING REPORT - PTI's", 6),
        ("EXPO LINE QC CHECKS", 6),
        ("HANDOVEER DAY", 7),
        ("HangarO", 7)
    ];
}