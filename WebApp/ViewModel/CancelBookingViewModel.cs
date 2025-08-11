using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModel
{
    public class CancelBookingViewModel
    {
        public int BookingId { get; set; }
        public string CancellationCode { get; set; }
    }
}
