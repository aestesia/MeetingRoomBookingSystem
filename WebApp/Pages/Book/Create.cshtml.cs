using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class CreateModel : PageModel
    {
        private readonly MyContext myContext;

        public CreateModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        [BindProperty]
        public CreateBookingViewModel BookingViewModel { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var employee = await myContext.Employees.FirstOrDefaultAsync(e => 
            e.Id == BookingViewModel.EmployeeId && e.EmployeeEmail == BookingViewModel.Email);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Employee ID and Email do not match.");
                return Page();
            }

            var room = await myContext.Rooms.FindAsync(BookingViewModel.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("BookingViewModel.RoomId", "Room does not exist.");
                return Page();
            }

            if(BookingViewModel.NumOfAttendees > room.Capacity)
            {
                ModelState.AddModelError("BookingViewModel.NumOfAttendees", $"Number of attendees exceeds room capacity ({room.Capacity}).");
                return Page();
            }

            var booking = new Booking
            {
                EmployeeId = BookingViewModel.EmployeeId,
                RoomId = BookingViewModel.RoomId,
                Title = BookingViewModel.Title,
                NumOfAttendees = BookingViewModel.NumOfAttendees,
                StartDate = BookingViewModel.StartDate,
                EndDate = BookingViewModel.EndDate,
                CancellationCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                isCancelled = false,
                SeriesId = Guid.NewGuid(),
                IsRecurring = BookingViewModel.IsRecurring
            };

            myContext.Bookings.Add(booking);
            await myContext.SaveChangesAsync();

            return RedirectToPage("/Home/Index");
        }
    }
}
