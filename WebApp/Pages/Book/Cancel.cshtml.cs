using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.ViewModel;

namespace WebApp.Pages.Book
{
    public class CancelModel : PageModel
    {
        private readonly MyContext myContext;

        public CancelModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        [BindProperty]
        public CancelBookingViewModel CancelBooking { get; set; }
        public string SuccessMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError(string.Empty, "No booking ID provided.");
                return Page();
            }

            var booking = await myContext.Bookings.FindAsync(id);

            if (booking == null)
            {
                ModelState.AddModelError(string.Empty, "Booking not found.");
                return Page();
            }

            CancelBooking = new CancelBookingViewModel
            {
                BookingId = booking.BookingId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var booking = await myContext.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == CancelBooking.BookingId);

            if (booking == null || booking.CancellationCode != CancelBooking.CancellationCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid Booking ID or Cancellation Code.");
                return Page();
            }

            if (booking.isCancelled)
            {
                ModelState.AddModelError(string.Empty, "This booking has already been cancelled.");
                return Page();
            }

            booking.isCancelled = true;
            await myContext.SaveChangesAsync();

            SuccessMessage = "Booking cancelled successfully.";
            ModelState.Clear();

            // Reset form
            CancelBooking = new CancelBookingViewModel();
            return Page();
        }
    }
}
