using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DDTV_WEB_Server.Pages
{
    public class LoginModel : PageModel
    {
        public int T1 = 0;
        public int test(object a)
        {
            T1++;
            return T1;
            //do something here
        }

        public void OnGet()
        {

        }
    }
}