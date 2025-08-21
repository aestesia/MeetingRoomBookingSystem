using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModel
{
    public class CancelBookingViewModel
    {
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }
        [Display(Name = "Update Code")]
        public string UpdateCode { get; set; }
    }
}
