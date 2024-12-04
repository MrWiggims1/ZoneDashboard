using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models.Trello
{
    public enum CardAreaOfOrigin
    {
        [Display(Name = "Welding")] Welding,
        [Display(Name = "Chassis")] Chassis,
        [Display(Name = "Bay 1")] Bay1,
        [Display(Name = "Bay 2")] Bay2,
        [Display(Name = "Bay 3")] Bay3,
        [Display(Name = "Bay 4")] Bay4,
        [Display(Name = "Electrical")] Electrical,
        [Display(Name = "Wall/Roof Mod")] WallRoofMod,
        [Display(Name = "Sealing")] Sealing,
        [Display(Name = "Toolbox")] Toolbox,
        [Display(Name = "Upholstery")] Upholstery,
        [Display(Name = "Commissioning")] Commissioning,
        [Display(Name = "Detailing")] Detailing,
        [Display(Name = "Cabs Finishing")] CabsFinishing,
        [Display(Name = "Paint Bay")] PaintBay,
        [Display(Name = "Sub Assembly")] SubAssembly,
        [Display(Name = "Cabs Assembly")] CabsAssembly,
        [Display(Name = "Cabs Prep")] CabsPrep,
        [Display(Name = "Stores")] Stores,
        [Display(Name = "Supplier")] Supplier,
        [Display(Name = "Contractor")] Contractor,
        [Display(Name = "Gas")] Gas,
        [Display(Name = "CNC")] CNC,
        [Display(Name = "One Comp")] OneComp,
        [Display(Name = "Design")] Design,
        [Display(Name = "QC")] QC,
        [Display(Name = "MODE")] Mode,
        [Display(Name = "Unknown")] Unknown
    }
}
