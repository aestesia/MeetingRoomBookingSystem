using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;

namespace WebApp.Pages.Base
{
    public class BasePageModel : PageModel
    {
        private readonly MyContext myContext;
        
        public BasePageModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        public List<SelectListItem> RoomList { get; set; } = new();

        public async Task LoadRoomListAsync()
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

        // Validate Booking (prime time, room capacity)
        public async Task<bool> ValidateBooking(DateTime startDate, DateTime endDate, int roomId, int numOfAttendees)
        {
            if(!ValidateDate(startDate, endDate))
                return false;
            if(!ValidatePrime(startDate, endDate))
                return false;
            if(!await ValidateRoom(roomId, numOfAttendees))
                return false;
            return true;
        }

        // Check Invalid Start Date and End Date
        public bool ValidateDate(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                ModelState.AddModelError(string.Empty, "Please enter valid Start Date and End Date");
                return false;
            }            
            return true;
        }

        // Check Prime Time
        public bool ValidatePrime(DateTime startDate, DateTime endDate)
        {
            DayOfWeek day = startDate.DayOfWeek;
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Friday)
            {
                var primeStart = startDate.Date.AddHours(9);
                var primeEnd = startDate.Date.AddHours(12);
                TimeSpan duration = endDate - startDate;
                if (startDate >= primeStart && startDate < primeEnd)
                {
                    if (duration > TimeSpan.FromHours(1))
                    {
                        ModelState.AddModelError(string.Empty, "Booking during prime time (9 AM – 12 PM) cannot exceed 1 hour.");
                        return false;
                    }
                }
            }
            
            return true;
        }

        // Check Room Capacity
        public async Task<bool> ValidateRoom(int roomId, int numOfAttendees)
        {
            var room = await myContext.Rooms.FindAsync(roomId);
            if (room == null)
            {
                ModelState.AddModelError("BookingViewModel.RoomId", "Room does not exist.");
                return false;
            }
            if (numOfAttendees > room.Capacity)
            {
                ModelState.AddModelError("BookingViewModel.NumOfAttendees", $"Number of attendees exceeds room capacity ({room.Capacity}).");
                return false;
            }
            return true;
        }

        // Check Conflict
        public async Task<(bool IsValid, string Error)> ValidateConflictAsync(DateTime start, DateTime end, int roomId)
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

            return (true, null);
        }

        // Make Suggestion
        public List<(DateTime Start, DateTime End)> FindNextAvail(int roomId, DateTime requestedStart, TimeSpan duration)
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

    }
}
