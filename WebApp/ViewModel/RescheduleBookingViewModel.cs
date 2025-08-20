using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModel
{
    public class RescheduleBookingViewModel
    {
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }
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
    }
}
