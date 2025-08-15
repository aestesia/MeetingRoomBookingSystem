using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModel
{
    public class CancelBookingViewModel
    {
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }
        [Display(Name = "Cancellation Code")]
        public string CancellationCode { get; set; }
    }
}
