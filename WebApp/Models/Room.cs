using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        public string Amenities { get; set; }

    }
}
