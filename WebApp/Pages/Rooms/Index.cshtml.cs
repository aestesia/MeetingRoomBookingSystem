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

        public async Task OnGetAsync()
        {
            Rooms = await myContext.Rooms.ToListAsync();
        }
    }
}
