using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly MyContext myContext;
        public IndexModel(MyContext myContext)
        {
            this.myContext = myContext;
        }

        public IList<Room> Rooms { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            int pageSize = 10;
            int totalRooms = await myContext.Rooms.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);
            CurrentPage = pageNumber;

            Rooms = await myContext.Rooms
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
