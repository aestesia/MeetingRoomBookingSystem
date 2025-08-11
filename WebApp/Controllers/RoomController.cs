using Microsoft.AspNetCore.Mvc;
using WebApp.Context;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class RoomController : Controller
    {
        MyContext myContext;

        public RoomController(MyContext myContext)
        {
            this.myContext = myContext;
        }

        // GET ALL
        public IActionResult Index()
        {
            var data = myContext.Rooms.ToList();
            return View(data);
        }

        //GET BY ID
        public IActionResult Details(int id)
        {
            var data = myContext.Rooms.Find(id);
            if (data == null) 
                return NotFound();

            return View(data);
        }

    }
}
