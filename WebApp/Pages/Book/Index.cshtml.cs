using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Pages.Book
{
    public class IndexModel : PageModel
    {
        private readonly MyContext myContext;
        public IndexModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        public IList<Booking> Bookings { get; set; }

        public async Task OnGetAsync()
        {
            Bookings = await myContext.Bookings
                .Include(x => x.Employee)
                .Include(x => x.Room)
                .Where(x => !x.isCancelled)
                .ToListAsync();
        }
    }

}
