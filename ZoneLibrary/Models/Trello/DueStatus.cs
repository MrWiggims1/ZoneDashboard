using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models.Trello
{
    public enum DueStatus
    {
        [Display(Name = "Not Due")] NotDue,
        [Display(Name = "Due Now")] Due,
        [Display(Name = "Over Due")] OverDue
    }
}
