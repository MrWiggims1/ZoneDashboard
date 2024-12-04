using DBLibrary.Models;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;

namespace ZoneProductionLibrary
{
    public static class TrelloUtil
    {
        private static Dictionary<VanModel, Regex> _vanNames = [];
        
        static TrelloUtil()
        {
            foreach (VanModel _vanType in Enum.GetValues<VanModel>())
            {
                string typeName = Enum.GetName(_vanType)!.ToLower();

                string pattern = "/" + typeName + @"\d\d\dr|" + typeName + @"\d\d\d|" + typeName + @"sr\d/";

                Regex regex = new Regex(pattern, RegexOptions.Compiled);
                
                _vanNames.Add(_vanType, regex);
            }
        }
        
        internal static TimeSpan GetTaskTime(Card card, IEnumerable<CustomField> customFields)
        {
            var desiredField = customFields.Single(x => x.Name == "TASK TIME (mins)");

            CustomFieldItem desiredFieldItem;

            if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
                desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
            else
                return TimeSpan.Zero;

            string fieldValue = desiredFieldItem.Value.NumberAsString;

            return TimeSpan.FromMinutes(double.Parse(fieldValue));
        }

        internal static CardStatus ToCardStatus(Card card, IEnumerable<CustomField> customFields, IEnumerable<CachedTrelloAction> customFieldActions, out DateTimeOffset? dateLastUpdated)
        {
            dateLastUpdated = null;

            var desiredField = customFields.Single(x => x.Name == "STATUS");

            CustomFieldItem desiredFieldItem;

            if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
                desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
            else
                return CardStatus.NotStarted;

            string fieldValue = desiredField.Options.Single(x => x.Id == desiredFieldItem.ValueId).Value.Text;
            customFieldActions = customFieldActions.Where(x => new string(x.ActionId.SkipLast(3).ToArray()) == new string (desiredFieldItem.Id.SkipLast(3).ToArray()));

            if(customFieldActions.Count() > 0)
                dateLastUpdated = customFieldActions.OrderBy(x => x.DateOffset).Last().DateOffset;

            switch (fieldValue)
            {
                case "COMPLETED":
                    return CardStatus.Completed;

                case "IN PROGRESS":
                    return CardStatus.InProgress;

                case "APPROVED DBL RED LINE":
                    return CardStatus.Completed; // TODO Confirm

                case "UNABLE TO COMPLETE":
                    return CardStatus.UnableToComplete;

                default:
                    Log.Logger.Error("Unknown card status {statusName}", fieldValue);
                    return CardStatus.Unknown;

            }
        }

        internal static CardAreaOfOrigin ToCardAreaOfOrigin(Card card, IEnumerable<CustomField> customFields)
        {
            var desiredFields = customFields.Where(x => x.Name == "Area of Origin" || x.Name == "Area of Origin:");

            IEnumerable<CustomFieldItem> desiredFieldItems;

            if (card.CustomFieldItems.Any(x => desiredFields.Any(y => y.Id == x.CustomFieldId)))
                desiredFieldItems = card.CustomFieldItems.Where(x => desiredFields.Any(y => y.Id == x.CustomFieldId));
            else
                return CardAreaOfOrigin.Unknown;

            string fieldValue = desiredFields
                                .SelectMany(x => x.Options)
                                .First(option => !string.IsNullOrEmpty(option.Value.Text) &&
                                                 desiredFieldItems.Any(cardItem => cardItem.ValueId == option.Id)).Value.Text;

            switch (fieldValue.ToLower())
            {
                // Gen 2 field values 
                case "welding":
                    return CardAreaOfOrigin.Welding;

                case "chassis":
                    return CardAreaOfOrigin.Chassis;

                case "bay 1 furniture install":
                    return CardAreaOfOrigin.Bay1;

                case "bay 1":
                    return CardAreaOfOrigin.Bay1;

                case "bay 2 electrical":
                    return CardAreaOfOrigin.Bay2;

                case "bay 2":
                    return CardAreaOfOrigin.Bay2;

                case "bay 3 wall/roof":
                    return CardAreaOfOrigin.Bay3;

                case "bay 3":
                    return CardAreaOfOrigin.Bay3;

                case "bay 4":
                    return CardAreaOfOrigin.Bay4;

                case "wall/roof mod":
                    return CardAreaOfOrigin.WallRoofMod;

                case "wall prep":
                    return CardAreaOfOrigin.WallRoofMod;

                case "wall mod":
                    return CardAreaOfOrigin.WallRoofMod;

                case "roof mod a":
                    return CardAreaOfOrigin.WallRoofMod;

                case "roof mod":
                    return CardAreaOfOrigin.WallRoofMod;

                case "roof":
                    return CardAreaOfOrigin.WallRoofMod;

                case "sealing":
                    return CardAreaOfOrigin.Sealing;

                case "bay 4 sealing":
                    return CardAreaOfOrigin.Sealing;

                case "toolbox":
                    return CardAreaOfOrigin.Toolbox;

                case "upholstery":
                    return CardAreaOfOrigin.Upholstery;

                case "commissioning":
                    return CardAreaOfOrigin.Commissioning;

                case "detailing":
                    return CardAreaOfOrigin.Detailing;

                case "bay 6":
                    return CardAreaOfOrigin.Commissioning;

                case "cabs finishing":
                    return CardAreaOfOrigin.CabsFinishing;

                case "paint pay":
                    return CardAreaOfOrigin.PaintBay;

                case "paint bay":
                    return CardAreaOfOrigin.PaintBay;

                case "sub assembly":
                    return CardAreaOfOrigin.SubAssembly;

                case "cabs prep":
                    return CardAreaOfOrigin.CabsPrep;

                case "cabs":
                    return CardAreaOfOrigin.CabsAssembly;

                case "cabs assembly":
                    return CardAreaOfOrigin.CabsAssembly;

                case "stores":
                    return CardAreaOfOrigin.Stores;

                case "supplier":
                    return CardAreaOfOrigin.Supplier;

                case "gas":
                    return CardAreaOfOrigin.Gas;

                case "contractor":
                    return CardAreaOfOrigin.Contractor;

                case "cnc":
                    return CardAreaOfOrigin.CNC;

                case "mode":
                    return CardAreaOfOrigin.Mode;

                case "one composite":
                    return CardAreaOfOrigin.OneComp;

                case "design":
                    return CardAreaOfOrigin.Design;

                case "qc":
                    return CardAreaOfOrigin.QC;

                case "bay4":
                    return CardAreaOfOrigin.Bay4;

                case "electrical":
                    return CardAreaOfOrigin.Electrical;

                case "unknown":
                    return CardAreaOfOrigin.Unknown;
                
                default:
                    if(fieldValue != "___")
                        Log.Logger.Error("Unknown card area of origin {area}", fieldValue);
                    return CardAreaOfOrigin.Unknown;
            }
        }

        internal static RedFlagIssue ToRedFlagIssue(Card card, IEnumerable<CustomField> customFields) //TODO: Handle multiple custom fields
        {
            var desiredField = customFields.Single(x => x.Name == "Red Flag Issue");

            CustomFieldItem desiredFieldItem;

            if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
                desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
            else
                return RedFlagIssue.Unspecified;

            string fieldValue = desiredField.Options.Single(x => x.Id == desiredFieldItem.ValueId).Value.Text;

            switch (fieldValue)
            {
                case "Workmanship":
                    return RedFlagIssue.WorkmanShip;

                case "Non Completed Task":
                    return RedFlagIssue.NonCompletedTask;

                case "Damage":
                    return RedFlagIssue.Damage;

                case "Out of Stock":
                    return RedFlagIssue.OutOfStock;

                case "Faulty Component":
                    return RedFlagIssue.FaultyComponent;

                case "Build Process":
                    return RedFlagIssue.BuildProcess;

                case "Design Issue":
                    return RedFlagIssue.DesignIssue;

                case "Missing Part":
                    return RedFlagIssue.MissingPart;

                case "Shortage":
                    return RedFlagIssue.Shortage;

                case "BOM":
                    return RedFlagIssue.BOM;

                case null:
                    return RedFlagIssue.Unspecified;

                default:
                    Log.Logger.Error("Red flag issue Not Defined {area}", fieldValue);
                    return RedFlagIssue.Unspecified;
            }
        }

        public static Color GetIndicatorColor(double completionRate, DueStatus status)
        {
            if (completionRate > .99d)
                return Color.Green;

            else if (status is DueStatus.NotDue)
                return Color.LightGray;

            else if (status is DueStatus.Due && completionRate == 0d)
                return Color.Black;
            
            else if (status is DueStatus.OverDue)
                return Color.Red;

            else if (completionRate > 0d)
                return Color.Orange;

            else
                return Color.Red;
        }

        public static Color GetIndicatorColor(bool isComplete, DueStatus status)
        {
            if (isComplete)
                return Color.Green;

            if (status == DueStatus.NotDue)
                return Color.LightGray;

            if (status == DueStatus.Due)
                return Color.Black;

            return Color.Red;
        }

        public static Color GetIndicatorColor(this CardStatus cardStatus)
        {
            if (cardStatus == CardStatus.Completed)
                return Color.Green;
            else if (cardStatus == CardStatus.InProgress)
                return Color.Orange;
            else if (cardStatus == CardStatus.NotStarted)
                return Color.Gray;
            else if (cardStatus == CardStatus.UnableToComplete)
                return Color.Red;
            else if (cardStatus == CardStatus.Unknown)
                return Color.Black;

            else
                return Color.Purple;
        }

        public static Color GetIndicatorColor(this double completionRate)
        {
            if (completionRate == 0)
                return Color.Red;

            else if (completionRate == 1)
                return Color.Green;

            else
                return Color.Orange;
        }

        public static bool TryGetVanName(string input, [NotNullWhen(true)] out VanModel? vanType, out string result)
        {
            string cleanName = input.ToLower().Replace("-", "").Split(' ')[0];

            VanModel? matchedType = null;
            List<string> vanNames = new List<string>();

            foreach (var regexPair in _vanNames)
            {
                MatchCollection matches = regexPair.Value.Matches(cleanName);

                if (matches.Count > 0)
                {
                    matchedType = regexPair.Key;
                }

                vanNames.AddRange(matches.Select(x => x.Value));
            }

            if (vanNames.Count == 0)
            {
                vanType = null;
                result = string.Empty;
                return false;
            }

            else if (vanNames.Count != 1)
            {
                Log.Logger.Error("Multiple vans found in input string {vans}, returning none.", String.Join(", ", vanNames));

                vanType = null;
                result = string.Empty;
                return false;
            }

            result = vanNames.First();
            vanType = matchedType;
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }
    }
}
