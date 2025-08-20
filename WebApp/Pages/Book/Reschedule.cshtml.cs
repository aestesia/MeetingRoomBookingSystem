using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Services;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class RescheduleModel : PageModel
    {
        private readonly MyContext myContext;
        private readonly EmailService emailService;

        public RescheduleModel (MyContext myContext, EmailService emailService)
        {
            this.myContext = myContext;
            this.emailService = emailService;
        }

        [BindProperty]
        public RescheduleBookingViewModel BookingViewModel { get; set; }
        public List<SelectListItem> RoomList { get; set; } = new();

        private async Task LoadRoomListAsync()
        {
            RoomList = await myContext.Rooms
                .OrderBy(r => r.RoomName)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoomName
                })
                .ToListAsync();
        }
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

        // Validate Booking
        private async Task<(bool IsValid, string Error)> ValidateBookingsAsync(DateTime start, DateTime end, int roomId)
        {

            // Check Booking Buffer Time
            bool isConflict = await myContext.Bookings.AnyAsync(x =>
                x.RoomId == roomId && !x.isCancelled &&
                (
                    start < x.EndDate.AddMinutes(15) &&
                    end > x.StartDate.AddMinutes(-15)
                )
            );
            if (isConflict)
                return (false, "Room is unavailable at the selected time due to an existing booking.");

            // Check Prime Time
            DayOfWeek day = start.DayOfWeek;
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Friday)
            {
                var primeStart = start.Date.AddHours(9);
                var primeEnd = start.Date.AddHours(12);
                TimeSpan duration = end - start;
                if (start >= primeStart && start < primeEnd)
                {
                    if (duration > TimeSpan.FromHours(1))
                        return (false, "Booking during prime time (9 AM – 12 PM) cannot exceed 1 hour.");
                }
            }

            return (true, null);
        }

        // Find Suggestion
        private List<(DateTime Start, DateTime End)> FindNextAvail(int roomId, DateTime requestedStart, TimeSpan duration)
        {
            const int bufferMins = 15;
            var buffer = TimeSpan.FromMinutes(bufferMins);
            var suggestions = new List<(DateTime Start, DateTime End)>();

            var dayStart = requestedStart.Date;
            var dayEnd = dayStart.AddDays(1);

            var bookings = myContext.Bookings
                .Where(x => x.RoomId == roomId && !x.isCancelled && x.StartDate < dayEnd && x.EndDate > dayStart)
                .OrderBy(x => x.StartDate)
                .ToList();

            DateTime gapStart = requestedStart + buffer;

            foreach (var booking in bookings)
            {
                if (booking.StartDate > gapStart)
                {
                    var gapDuration = booking.StartDate - gapStart;
                    if (gapDuration >= duration)
                    {
                        suggestions.Add((gapStart, gapStart + duration));
                        if (suggestions.Count == 3)
                            break;
                    }
                }
                if (booking.EndDate > gapStart)
                    gapStart = booking.EndDate;
            }
            if (suggestions.Count < 3 && dayEnd - gapStart >= duration)
                suggestions.Add((gapStart, gapStart + duration));

            return suggestions;
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

            // Invalid Start Date and End Date
            if (BookingViewModel.EndDate <= BookingViewModel.StartDate)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid Start Date and End Date");
                return Page();
            }

            // Check Room Capacity
            var room = await myContext.Rooms.FindAsync(BookingViewModel.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("BookingViewModel.RoomId", "Room does not exist.");
                return Page();
            }
            if (BookingViewModel.NumOfAttendees > room.Capacity)
            {
                ModelState.AddModelError("BookingViewModel.NumOfAttendees", $"Number of attendees exceeds room capacity ({room.Capacity}).");
                return Page();
            }

            var (isValid, errorMessage) = await ValidateBookingsAsync(BookingViewModel.StartDate, BookingViewModel.EndDate, BookingViewModel.RoomId);
            if (!isValid)
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
