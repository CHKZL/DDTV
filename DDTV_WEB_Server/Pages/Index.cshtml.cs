using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DDTV_WEB_Server.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; private set; } = "In page model: ";

        public void OnGet()
        {
            Message += $" µ±Ç°ÃëÊý:  { DateTime.Now.Second.ToString() }";
        }
    }
}
