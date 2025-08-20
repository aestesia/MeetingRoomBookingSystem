using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class DetailsModel : PageModel
    {
        private readonly MyContext myContext;

        public DetailsModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        public GetBookingViewModel? booking {  get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            booking = await myContext.Bookings
                .Where(x => x.BookingId == id && !x.isCancelled)
                .Select(x => new GetBookingViewModel
                {
                    BookingId = x.BookingId,
                    Title = x.Title,
                    RoomName = x.Room.RoomName,
                    BookedBy = x.Employee.EmployeeName,
                    NumOfAttendees = x.NumOfAttendees,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsRecurring = x.IsRecurring
                })
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
