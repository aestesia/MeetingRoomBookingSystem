using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Pages.Base;
using WebApp.Services;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class RescheduleModel : BasePageModel
    {
        private readonly MyContext myContext;
        private readonly EmailService emailService;

        public RescheduleModel (MyContext myContext, EmailService emailService)
            :base(myContext)
        {
            this.myContext = myContext;
            this.emailService = emailService;
        }

        [BindProperty]
        public RescheduleBookingViewModel BookingViewModel { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadRoomListAsync();

            if (id == null)
            {
                ModelState.AddModelError(string.Empty, "No booking ID provided.");
                return Page();
            }

            var booking = await myContext.Bookings
                .Include(x => x.Employee)
                .Include(x => x.Room)
                .SingleOrDefaultAsync(x => x.BookingId == id && !x.isCancelled);

            if (booking == null)
            {
                ModelState.AddModelError(string.Empty, "Booking not found.");
                return Page();
            }

            BookingViewModel = new RescheduleBookingViewModel
            {
                BookingId = booking.BookingId,
                Email = booking.Employee.EmployeeEmail,
                RoomId = booking.Room.Id,
                Title = booking.Title,
                NumOfAttendees = booking.NumOfAttendees,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadRoomListAsync();

            if (!ModelState.IsValid)
                return Page();

            var booking = await myContext.Bookings
                .Include(x => x.Employee)
                .Include(x => x.Room)
                .SingleOrDefaultAsync(x => x.BookingId == BookingViewModel.BookingId && !x.isCancelled);

            // Validate Booking
            var isValid = await ValidateBooking(BookingViewModel.StartDate, BookingViewModel.EndDate,
                BookingViewModel.RoomId, BookingViewModel.NumOfAttendees);
            if (!isValid)
                return Page();

            // Validate occurences
            var (isNotConflict, errorMessage) = await ValidateConflictAsync(BookingViewModel.StartDate, BookingViewModel.EndDate, BookingViewModel.RoomId);
            if (!isNotConflict)
            {
                var duration = BookingViewModel.EndDate - BookingViewModel.StartDate;
                var suggestions = FindNextAvail(BookingViewModel.RoomId, BookingViewModel.StartDate, duration);

                if (suggestions.Any())
                {
                    ModelState.AddModelError(string.Empty, $"{errorMessage} Here are some available time slots you can consider:");
                    ViewData["ConflictSuggestions"] = suggestions;
                }
                else
                    ModelState.AddModelError(string.Empty, errorMessage);
                return Page();
            }

            booking.RoomId = BookingViewModel.RoomId;
            booking.NumOfAttendees = BookingViewModel.NumOfAttendees;
            booking.StartDate = BookingViewModel.StartDate;
            booking.EndDate = BookingViewModel.EndDate;

            await myContext.SaveChangesAsync();

            return RedirectToPage("/Home/Index");
        }
    }
}
