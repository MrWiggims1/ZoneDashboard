using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models.Trello
{
    public enum RedFlagIssue
    {
        [Display(Name = "Workmanship")] WorkmanShip,
        [Display(Name = "Non Completed Task")] NonCompletedTask,
        [Display(Name = "Damage")] Damage,
        [Display(Name = "Out Of Stock")] OutOfStock,
        [Display(Name = "Faulty Component")] FaultyComponent,
        [Display(Name = "Build Process")] BuildProcess,
        [Display(Name = "Design Issue")] DesignIssue,
        [Display(Name = "Missing Part")] MissingPart,
        [Display(Name = "Shortage")] Shortage,
        [Display(Name = "BOM")] BOM,
        [Display(Name = "Unspecified")] Unspecified
    }
}
