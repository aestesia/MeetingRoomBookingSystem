using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;

namespace WebApp.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly MyContext myContext;

        public IndexModel(ILogger<IndexModel> logger, MyContext myContext)
        {
            this._logger = logger;
            this.myContext = myContext;
        }

        public class CalenderEvent
        {
            public int id { get; set; }
            public string title { get; set; }
            public string bookedBy { get; set; }
            public string room { get; set; }
            public string start { get; set; }
            public string end { get; set; }
        }

        public List<string> RoomNames { get; set; }
        public List<CalenderEvent> CalenderEvents { get; set; }

        public async Task OnGetAsync()
        {
            var bookings = await myContext.Bookings
                .Include(x => x.Room)
                .Include(x => x.Employee)
                .Where(x => !x.isCancelled)
                .ToListAsync();

            RoomNames = bookings
                .Select(b => b.Room.RoomName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            CalenderEvents = bookings.Select(x => new CalenderEvent
            {
                id = x.BookingId,
                title = x.Title,
                bookedBy = x.Employee.EmployeeName,
                room = x.Room.RoomName,
                start = x.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = x.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();
        }
    }
}
