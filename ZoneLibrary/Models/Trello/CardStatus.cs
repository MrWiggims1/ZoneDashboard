using System.ComponentModel.DataAnnotations;

namespace ZoneProductionLibrary.Models.Trello
{
    public enum CardStatus
    {
        [Display(Name = "Completed")] Completed,
        [Display(Name = "In Progress")] InProgress,
        [Display(Name = "Not Started")] NotStarted,
        [Display(Name = "Unable To Complete")] UnableToComplete,
        [Display(Name = "Unknown")] Unknown
    }
}
