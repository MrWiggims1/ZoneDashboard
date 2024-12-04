namespace ZoneProductionLibrary.Models.ProductionPosition;

public class Gen2ProductionPosition : IProductionPosition
{
    public bool IsInProduction => true;
    public bool IsInCarPark(HandoverState _) => false;

    public string PositionName => Positions.Single(x => x.id == PositionId).name;
    public string ShortPositionName => Positions.Single(x => x.id == PositionId).shortName;
    public int PositionId { get; }
    public override string ToString() => $"{PositionName} {PositionId}";

    public static IProductionPosition operator +(Gen2ProductionPosition position, int add)
    {
        if (position.PositionId + add > Positions.Last().id) return new PostProduction();

        string newPos = Positions.Single(x => x.id == position.PositionId + add).name;
        return new Gen2ProductionPosition(ProductionPositionEntryType.LineMoveName, newPos);
    }

    public static IProductionPosition operator -(Gen2ProductionPosition position, int subtract)
    {
        if (position.PositionId - subtract < 0) return new PreProduction();

        string newPos = Positions.Single(x => x.id == position.PositionId - subtract).name;
        return new Gen2ProductionPosition(ProductionPositionEntryType.LineMoveName, newPos);
    }

    public bool Equals(IProductionPosition? other)
    {
        if (other is Gen2ProductionPosition gen2Pos)
            return gen2Pos.PositionId == PositionId;

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
        if(pos is Gen2ProductionPosition gen2)
            return HashCode.Combine(nameof(Gen2ProductionPosition), gen2.PositionId);

        throw new ArgumentException("Position is not Gen2");
    }

    private Gen2ProductionPosition(int positionId)
    {
        if(Positions.All(x => x.id != positionId))
            throw new ArgumentOutOfRangeException(nameof(positionId), $"Gen 2 production does not contain a value with id {positionId}");

        PositionId = positionId;
    }

    public static Gen2ProductionPosition First => new Gen2ProductionPosition(0);
    public static Gen2ProductionPosition Last => new Gen2ProductionPosition(Positions.Last().id);

    public static IEnumerable<IProductionPosition> GetAll()
    {
        List<IProductionPosition> results = [];

        foreach (var position in Positions)
        {
            results.Add(new Gen2ProductionPosition(position.id));
        }

        return results;
    }
    
    private Gen2ProductionPosition(ProductionPositionEntryType type, string name)
    {
        if (type == ProductionPositionEntryType.LineMoveName)
            this.PositionId = Positions.Single(x => x.name == name).id;

        else
            this.PositionId = BoardPositions.First(x => x.name == name).id;
    }

    public static bool TryGetGen2Position(ProductionPositionEntryType type, string name, out IProductionPosition? position)
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
                position = new Gen2ProductionPosition(type, name);
                return true;
            }
        }

        else if (BoardPositions.Any(x => x.name == name))
        {
            position = new Gen2ProductionPosition(type, name);
            return true;
        }
        
        Log.Logger.Debug("{listName} is not a valid Gen 2 list name", name);

        position = null;
        return false;
    }

    public static readonly (string name, string shortName, int id)[] Positions =
    [
        ("Chassis Module Gen2", "Chassis", 0),
        ("BAY 1 - Funnel", "Funnel", 1),
        ("BAY 1 Furniture install", "Bay 1", 2),
        ("BAY 2 Electrical", "Bay 2", 3),
        ("BAY 3 Wall & Roof", "Bay 3", 4),
        ("BAY 4", "Bay 4", 5),
        ("BAY 5 Sealing", "Bay 5", 6),
        ("BAY 6", "Bay 6", 7),
        ("BAY 7 Commissioning", "Bay 7", 8),
        ("BAY 8 Commissioning", "Bay 8", 9),
        ("Outside BAY 8", "Outside Bay 8", 10),
        ("RAIN MAKER", "Rain Maker", 11),
        ("Dome - GEN2", "Dome", 12),
    ];

    public static readonly (string name, int id)[] BoardPositions =
    [
        ("CHASSIS MODULE", 0),
        ("[QC] Chassis Checklist", 0),
        ("WELDING", 0),
        ("SUB ASSAMBLY", 0),
        ("SUB ASSAMBLY (REV001)", 0),
        ("SUB ASSAMBLY (REV002)", 0),
        ("SUB ASSAMBLY (REV003)", 0),
        ("CABS PREP", 0),
        ("PAINT BAY", 0),
        ("[QC] Paint Bay Checklist", 0),
        ("CABs MODULE", 0),
        ("CABs MODULE (REV001)", 0),
        ("CABs MODULE (REV002)", 0),
        ("CABs MODULE (REV003)", 0),
        ("CABs MODULE (REV004)", 0),
        ("CABs MODULE (REV005)", 0),
        ("QA Cabs Gen2 Checklist (REV002)", 1),
        ("QA Cabs Gen2 Checklist", 1),
        ("[QA] Cabs Gen2 Checklist", 1),
        ("[QC] CABs Checklist", 1),
        ("BAY 1 FURNITURE INSTALL (REV001)", 2),
        ("BAY 1 FURNITURE INSTALL (REV002)", 2),
        ("BAY 1 FURNITURE INSTALL (REV003)", 2),
        ("BAY 1 FURNITURE INSTALL", 2),
        ("[QC] Bay1 Checklist", 2),
        ("Bay Leader Inspection Bay1", 2),
        ("BAY 1 FURNITURE INSTALL changed", 2),
        ("BAY 2 ELECTRICAL(REV001)", 3),
        ("BAY 2 ELECTRICAL(REV002)", 3),
        ("BAY 2 ELECTRICAL(REV003)", 3),
        ("BAY 2 ELECTRICAL (REV001)", 3),
        ("BAY 2 ELECTRICAL (REV002)", 3),
        ("BAY 2 ELECTRICAL (REV003)", 3),
        ("BAY 2 ELECTRICAL", 3),
        ("BAY 2 ELECTRICAL ", 3),
        ("[QC] Bay2 Checklist", 3),
        ("[QA] Cabs Gen2 Checklist", 3),
        ("WALL/ROOF MOD (REV001)", 3),
        ("WALL/ROOF MOD (REV002)", 3),
        ("WALL/ROOF MOD (REV003)", 3),
        ("WALL/ROOF MOD", 3),
        ("[QC] Wall/Roof Mod Checklist", 3),
        ("WALL/ROOF MOD QC", 3),
        ("Bay Leader Inspection Wall/Roof Mod", 3),
        ("MODE", 3), //TODO: Confirm Position
        ("[QC] MODE", 3), //TODO: Confirm Position
        ("BAY 3 WALL/ROOF INSTALL", 4),
        ("[QC] Bay3 Checklist", 4),
        ("BAY 3 WALL/ROOF INSTALL changed", 4),
        ("BAY 4 SEALING & ELECTRICAL (REV001)", 5),
        ("BAY 4 SEALING & ELECTRICAL (REV002)", 5),
        ("BAY 4 SEALING & ELECTRICAL (REV003)", 5),
        ("BAY 4 SEALING & ELECTRICAL", 5),
        ("BAY 4  & ELECTRICAL", 5),
        ("BAY 5 (REV001)", 6),
        ("[QC] Bay4 Checklist", 6),
        ("BAY 5 (REV002)", 6),
        ("BAY 5 (REV003)", 6),
        ("BAY 5", 6),
        ("Upholstery", 6),
        ("Bay 6 Upholstery", 7),
        ("BAY 6 Upholstery", 7),
        ("[QC] Bay6 Upholstery Checklist", 7),
        ("BAY 5 SEALING", 6),
        ("BAY 6 COMMISSIONING REPORT - PTI's", 7),
        ("BAY 6 COMMISSIONING", 7),
        ("BAY 6 DETAILING PTI's", 7),
        ("BAY 4/5/6 QA (REV003)", 7),
        ("Bay 7 - External Commissioning", 8),
        ("Bay 7 - COMMISSIONING", 8),
        ("Bay 8 - Outside Commissioning", 10),
        ("Bay 7 Commissioning PTIs", 8),
        ("Bay 8 Commissioning PTIs", 9),
        ("Bay 8 - COMMISSIONING", 9),
        ("Bay 9 Outside Commissioning PTIs", 10),
        ("DETAILING", 10),
        ("BAY 6 DETAILING PTI's😅😊😊😙😚", 8),
        ("PRO-HO CHECKS", 11)
    ];
}