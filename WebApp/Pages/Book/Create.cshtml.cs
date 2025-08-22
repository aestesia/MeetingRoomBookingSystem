using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;
using WebApp.Pages.Base;
using WebApp.Services;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class CreateModel : BasePageModel
    {
        private readonly MyContext myContext;
        private readonly EmailService emailService;

        public CreateModel(MyContext myContext, EmailService emailService)
            : base(myContext)
        {
            this.myContext = myContext;
            this.emailService = emailService;
        }

        [BindProperty]
        public CreateBookingViewModel BookingViewModel { get; set; }        

        public async Task OnGetAsync()
        {
            await LoadRoomListAsync();
        }
        
        // Recurrence
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
            await LoadRoomListAsync();

            if (!ModelState.IsValid)
                return Page();

            var employee = await myContext.Employees.FirstOrDefaultAsync(e => 
            e.Id == BookingViewModel.EmployeeId && e.EmployeeEmail == BookingViewModel.Email);

            // Employee ID and Email do not match
            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Employee ID and Email do not match.");
                return Page();
            }

            // Validate Booking
            var isValid = await ValidateBooking(BookingViewModel.StartDate, BookingViewModel.EndDate, 
                BookingViewModel.RoomId, BookingViewModel.NumOfAttendees);
            if (!isValid)
                return Page();            

            //Handle Recurrence
            List<(DateTime Start, DateTime End)> occurrences;
            if (BookingViewModel.IsRecurring)
            {
                if (!BookingViewModel.RecurrenceEndDate.HasValue || 
                    BookingViewModel.RecurrenceEndDate <= BookingViewModel.StartDate)
                {
                    ModelState.AddModelError("BookingViewModel.RecurrenceEndDate", "Invalid recurrence end date.");
                    return Page();
                }

                occurrences = GenerateOccurrences(
                    BookingViewModel.StartDate,
                    BookingViewModel.EndDate,
                    BookingViewModel.RecurrencePattern,
                    BookingViewModel.RecurrenceEndDate.Value.AddDays(1)
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
                var (isNotConflict, errorMessage) = await ValidateConflictAsync(start, end, 0, BookingViewModel.RoomId);
                if (!isNotConflict)
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

            // Add to Database
            var seriesId = Guid.NewGuid();
            var updateCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
            var room = await myContext.Rooms.SingleOrDefaultAsync(x => x.Id == BookingViewModel.RoomId);
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
                    UpdateCode = updateCode,
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
                room: room.RoomName,
                startDate: firstBook.StartDate.ToString("yyyy-MM-dd HH:mm"),
                endDate: firstBook.EndDate.ToString("yyyy-MM-dd HH:mm"),
                updateCode: firstBook.UpdateCode
            );

            TempData["SuccessMsg"] = "Booking created successfully";
            return RedirectToPage("Create");
        }
    }
}
