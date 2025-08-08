using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        [ForeignKey("Emloyee")]
        public int EmployeeId { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public int NumOfAttendees { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CancellationCode { get; set; }
        public Guid? SeriesId { get; set; } 

        public Employee Employee { get; set; }
        public Room Room { get; set; }
    }
}
