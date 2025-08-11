using Microsoft.AspNetCore.Mvc;
using WebApp.Context;

namespace WebApp.Controllers
{
    public class BookingController : Controller
    {
        MyContext myContext;

        public BookingController(MyContext myContext)
        {
            this.myContext = myContext;
        }

    }
}
