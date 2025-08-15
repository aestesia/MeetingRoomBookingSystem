using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModel
{
    public enum RecurrencePattern
    {
        Daily,
        Weekly,
        Monthly
    }
    public class CreateBookingViewModel
    {
        [Display(Name = "Employee ID")]
        public int EmployeeId { get; set; }
        public string Email { get; set; }
        [Display(Name = "Room")]
        public int RoomId { get; set; }
        public string Title { get; set; }
        [Display(Name = "Number of Attendees")]
        public int NumOfAttendees { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Is Recurring")]
        public bool IsRecurring { get; set; }
        [Display(Name = "Recurrence Pattern")]
        public RecurrencePattern RecurrencePattern { get; set; }
        [Display(Name = "Recurrence End Date")]
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
