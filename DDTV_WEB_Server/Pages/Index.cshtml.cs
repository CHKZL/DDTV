using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DDTV_WEB_Server.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; private set; } = "In page model: ";

        public void OnGet()
        {
            Message += $" 当前秒数:  { DateTime.Now.Second.ToString() }";
        }
        public void AutoReceipt()
        {
            Message += $" 当前秒数:  { DateTime.Now.Second.ToString() }";
        }

        public IActionResult OnPostSave()
        {
            return RedirectToPage("List");
        }
    }
}
