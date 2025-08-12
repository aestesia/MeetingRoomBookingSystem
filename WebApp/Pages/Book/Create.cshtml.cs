using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task<IActionResult> OnPostAsync()
        {
            var employee = await myContext.Employees.FirstOrDefaultAsync(e => 
            e.Id == BookingViewModel.EmployeeId && e.EmployeeEmail == BookingViewModel.Email);
            
            var room = await myContext.Rooms.FindAsync(BookingViewModel.RoomId);

            if (!ModelState.IsValid)
                return Page();

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

            // Check Booking Buffer Time
            var isConflict = await myContext.Bookings.AnyAsync(x =>
                x.RoomId == BookingViewModel.RoomId && !x.isCancelled &&
                (
                    BookingViewModel.StartDate < x.EndDate.AddMinutes(15) &&
                    BookingViewModel.EndDate > x.StartDate.AddMinutes(-15)
                )
            );
            if (isConflict)
            {
                ModelState.AddModelError(string.Empty, "The selected room is not available due to another booking.");
                return Page();
            }

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
                    {
                        ModelState.AddModelError(string.Empty, "Booking from prime time (9 AM - 12 PM) cannot exceed 1 hour.");
                        return Page();
                    }
                }
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

            await emailService.SendBookingConfirmationAsync(
                toEmail: BookingViewModel.Email,
                employeeName: employee.EmployeeName,
                bookingId: booking.BookingId.ToString(),
                title: booking.Title,
                startDate: booking.StartDate.ToString("yyyy-MM-dd HH:mm"), 
                endDate: booking.EndDate.ToString("yyyy-MM-dd HH:mm"),
                cancellationCode: booking.CancellationCode
            );

            return RedirectToPage("/Home/Index");
        }
    }
}
