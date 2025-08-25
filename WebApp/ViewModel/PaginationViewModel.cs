namespace WebApp.ViewModel
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string PageRoute { get; set; } = string.Empty;
    }
}
