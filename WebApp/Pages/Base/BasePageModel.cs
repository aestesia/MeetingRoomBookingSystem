using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Base
{
    public class BasePageModel : PageModel
    {
        private const string TempDataSuccessKey = "successMsg";
        private const string TempDataErrorKey = "ErrorMsg";

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        protected void SetSuccessMessage(string message)
        {
            SuccessMessage = message;
        }

        protected void SetErrorMessage(string message)
        {
            ErrorMessage = message;
        }
        protected void ClearMessages()
        {
            SuccessMessage = null;
            ErrorMessage = null;
        }
    }
}
