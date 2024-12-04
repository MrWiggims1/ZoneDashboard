using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models
{
    public enum VanModel
    {
        ZPP,
        ZSP,
        ZSPF,
        ZSS,
        ZSSF,
        EXP
    }
    
    public enum TypeOfVan
    { 
        [Display(Description = "Gen 2")] Gen2,
        [Display(Description = "Expo")] Expo
    }
}