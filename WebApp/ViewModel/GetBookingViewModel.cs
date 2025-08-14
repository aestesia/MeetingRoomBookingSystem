using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Models;

namespace WebApp.ViewModel
{
    public class GetBookingViewModel
    {
        public int BookingId { get; set; }
        public string Title { get; set; }
        public string RoomName { get; set; }
        public string BookedBy { get; set; }
        public int NumOfAttendees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsRecurring { get; set; }
    }
}
