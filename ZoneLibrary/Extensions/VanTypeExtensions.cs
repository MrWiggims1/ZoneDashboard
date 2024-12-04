using Serilog;
using System.Drawing;
using ZoneProductionLibrary.Models.Boards;

namespace ZoneProductionLibrary.Extensions
{
    static public class VanTypeExtensions
    {
        public static bool IsGen2(this VanModel vanModel)
        {
            return vanModel is VanModel.ZPP or VanModel.ZSP or VanModel.ZSPF or VanModel.ZSS or VanModel.ZSSF;
        }
        
        
        public static TypeOfVan VanType(this VanModel vanModel)
        {
            if (vanModel.IsGen2())
                return TypeOfVan.Gen2;
            
            else
                return TypeOfVan.Expo;
        }

        public static VanModel ToVanType(this string input)
        {
            TrelloUtil.TryGetVanName(input, out var type, out _);

            if (type is null)
                throw new ArgumentException(nameof(input), $"{input} is not a valid van type");
                
            return type.Value;
        }

        public static Color GetTypeColor(this VanModel vanModel)
        {
            switch (vanModel)
            {
                case VanModel.ZSP:  return ColorTranslator.FromHtml("#fb5607");
                case VanModel.ZPP:  return ColorTranslator.FromHtml("#ffbe0b");
                case VanModel.ZSPF: return ColorTranslator.FromHtml("#ff006e");
                case VanModel.ZSS:  return ColorTranslator.FromHtml("#47BCFF");
                case VanModel.ZSSF: return ColorTranslator.FromHtml("#47BCFF");
                case VanModel.EXP:  return ColorTranslator.FromHtml("#A26AF1");

                default:
                    {
                        Log.Logger.Error("{VanModel} does not have a color yet.", Enum.GetName(vanModel));
                        throw new ArgumentOutOfRangeException();
                    }
            }
        }
    }
}
