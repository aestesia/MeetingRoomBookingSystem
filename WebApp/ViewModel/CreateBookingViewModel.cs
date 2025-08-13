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
        public int EmployeeId { get; set; }
        public string Email { get; set; }
        public int RoomId { get; set; }
        public string Title { get; set; }
        public int NumOfAttendees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
