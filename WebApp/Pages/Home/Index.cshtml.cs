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
            public string title { get; set; }
            public string start { get; set; }
            public string end { get; set; }
        }

        public List<CalenderEvent> CalenderEvents { get; set; } = new();

        public async Task OnGetAsync()
        {
            var bookings = await myContext.Bookings
                .Include(x => x.Room)
                .Include(x => x.Employee)
                .Where(x => !x.isCancelled)
                .ToListAsync();

            CalenderEvents = bookings.Select(x => new CalenderEvent
            {
                title = $"{x.Title} - {x.Room.RoomName} (Booked by: {x.Employee.EmployeeName})",
                start = x.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = x.EndDate.ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();
        }
    }
}
