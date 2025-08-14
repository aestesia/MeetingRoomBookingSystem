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
    public class CreateModel : PageModel
    {
        private readonly MyContext myContext;
        private readonly EmailService emailService;

        public CreateModel(MyContext myContext, EmailService emailService)
        {
            this.myContext = myContext;
            this.emailService = emailService;
        }

        [BindProperty]
        public CreateBookingViewModel BookingViewModel { get; set; }
        public List<SelectListItem> RoomList { get; set; } = new();

        public async Task OnGetAsync()
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

        private async Task<(bool IsValid, string Error)> ValidateBookingsAsync(DateTime start, DateTime end, int roomId)
        {
            // Check Booking Buffer Time
            bool isConflict = await myContext.Bookings.AnyAsync(x =>
                x.RoomId == BookingViewModel.RoomId && !x.isCancelled &&
                (
                    BookingViewModel.StartDate < x.EndDate.AddMinutes(15) &&
                    BookingViewModel.EndDate > x.StartDate.AddMinutes(-15)
                )
            );
            if (isConflict)
                return (false, "Room is unavailable at the selected time due to an existing booking.");

            // Check Prime Time
            DayOfWeek day = BookingViewModel.StartDate.DayOfWeek;
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Friday)
            {
                var primeStart = BookingViewModel.StartDate.Date.AddHours(9);
                var primeEnd = BookingViewModel.StartDate.Date.AddHours(12);
                TimeSpan duration = BookingViewModel.EndDate - BookingViewModel.StartDate;
                if (BookingViewModel.StartDate >= primeStart && BookingViewModel.StartDate < primeEnd)
                {
                    if (duration > TimeSpan.FromHours(1))
                        return (false, "Booking during prime time (9 AM – 12 PM) cannot exceed 1 hour.");
                }
            }

            return (true, null);
        }

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
                if(booking.EndDate > gapStart)
                    gapStart = booking.EndDate;
            }
            if (suggestions.Count < 3 && dayEnd - gapStart >= duration)
                suggestions.Add((gapStart, gapStart + duration));

            return suggestions;
        }
        private List<(DateTime Start, DateTime End)> GenerateOccurrences(
            DateTime start,
            DateTime end,
            RecurrencePattern pattern,
            DateTime recurrenceEnd)
        {
            var occurences = new List<(DateTime Start, DateTime End)>();
            var duration = end - start;
            var currentStart = start;

            while (currentStart <= recurrenceEnd)
            {
                var currentEnd = currentStart + duration;
                
                occurences.Add((currentStart, currentEnd));

                currentStart = pattern switch
                {
                    RecurrencePattern.Daily => currentStart.AddDays(1),
                    RecurrencePattern.Weekly => currentStart.AddDays(7),
                    RecurrencePattern.Monthly => currentStart.AddMonths(1),
                    _ => throw new ArgumentOutOfRangeException(nameof(pattern))
                };
            }
            return occurences;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RoomList = await myContext.Rooms
                .OrderBy(r => r.RoomName)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoomName
                })
                .ToListAsync();

            if (!ModelState.IsValid)
                return Page();

            var employee = await myContext.Employees.FirstOrDefaultAsync(e => 
            e.Id == BookingViewModel.EmployeeId && e.EmployeeEmail == BookingViewModel.Email);
            
            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Employee ID and Email do not match.");
                return Page();
            }

            if (BookingViewModel.StartDate > BookingViewModel.EndDate)
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

            //Handle Recurrence
            List<(DateTime Start, DateTime End)> occurrences;
            if (BookingViewModel.IsRecurring)
            {
                if (!BookingViewModel.RecurrenceEndDate.HasValue || BookingViewModel.RecurrenceEndDate <= BookingViewModel.StartDate)
                {
                    ModelState.AddModelError("BookingViewModel.RecurrenceEndDate", "Invalid recurrence end date.");
                    return Page();
                }

                occurrences = GenerateOccurrences(
                    BookingViewModel.StartDate,
                    BookingViewModel.EndDate,
                    BookingViewModel.RecurrencePattern,
                    BookingViewModel.RecurrenceEndDate.Value
                );
            }
            else
            {
                occurrences = new List<(DateTime Start, DateTime End)>
                {
                    (BookingViewModel.StartDate, BookingViewModel.EndDate)
                };
            }

            // Validate all occurences
            foreach (var (start, end) in occurrences) 
            {
                var (isValid, errorMessage) = await ValidateBookingsAsync(start, end, BookingViewModel.RoomId);
                if (!isValid)
                {
                    var duration = end - start;
                    var suggestions = FindNextAvail(BookingViewModel.RoomId, start, duration);

                    if (suggestions.Any())
                    {
                        ModelState.AddModelError(string.Empty, $"{errorMessage} Here are some available time slots you can consider:");
                        ViewData["ConflictSuggestions"] = suggestions;
                    }
                    else
                        ModelState.AddModelError(string.Empty, errorMessage);
                    return Page();
                }
            }            

            var seriesId = Guid.NewGuid();
            Booking firstBook = null;
            foreach (var (start, end) in occurrences)
            {
                var booking = new Booking
                {
                    EmployeeId = BookingViewModel.EmployeeId,
                    RoomId = BookingViewModel.RoomId,
                    Title = BookingViewModel.Title,
                    NumOfAttendees = BookingViewModel.NumOfAttendees,
                    StartDate = start,
                    EndDate = end,
                    CancellationCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    isCancelled = false,
                    SeriesId = seriesId,
                    IsRecurring = BookingViewModel.IsRecurring
                };

                myContext.Bookings.Add(booking);
                if (firstBook == null)
                    firstBook = booking;
            }
            await myContext.SaveChangesAsync();

            await emailService.SendBookingConfirmationAsync(
                toEmail: BookingViewModel.Email,
                employeeName: employee.EmployeeName,
                bookingId: firstBook.BookingId.ToString(),
                title: firstBook.Title,
                startDate: firstBook.StartDate.ToString("yyyy-MM-dd HH:mm"),
                endDate: firstBook.EndDate.ToString("yyyy-MM-dd HH:mm"),
                cancellationCode: firstBook.CancellationCode
            );

            return RedirectToPage("/Home/Index");
        }
    }
}
