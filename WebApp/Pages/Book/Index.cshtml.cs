using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class IndexModel : PageModel
    {
        private readonly MyContext myContext;
        public IndexModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        public IList<GetBookingViewModel> Bookings { get; set; }
        public string CurrentSort { get; set; }

        public async Task OnGetAsync(string sortOrder)
        {
            CurrentSort = sortOrder;

            var query = myContext.Bookings
                .Where(x => !x.isCancelled)
                .Select(x => new GetBookingViewModel
                {
                    BookingId = x.BookingId,
                    Title = x.Title,
                    RoomName = x.Room.RoomName,
                    BookedBy = x.Employee.EmployeeName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                });
            //.ToListAsync();

            query = sortOrder switch
            {
                "title_desc" => query.OrderByDescending(x => x.Title),
                "bookedby_asc" => query.OrderBy(x => x.BookedBy),
                "bookedby_desc" => query.OrderByDescending(x => x.BookedBy),
                "room_asc" => query.OrderBy(x => x.RoomName),
                "room_desc" => query.OrderByDescending(x => x.RoomName),
                "startdate_asc" => query.OrderBy(x => x.StartDate),
                "startdate_desc" => query.OrderByDescending(x => x.StartDate),
                "enddate_asc" => query.OrderBy(x => x.EndDate),
                "enddate_desc" => query.OrderByDescending(x => x.EndDate),
                _ => query.OrderBy(x => x.Title),
            };

            Bookings = await query.ToListAsync();

        }
    }

}
