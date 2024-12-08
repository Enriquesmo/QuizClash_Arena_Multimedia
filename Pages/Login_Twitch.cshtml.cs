using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Login_TwitchModel : PageModel
    {
        // Propiedad p�blica para almacenar el ganador
        public string? auth { get; set; }

       
        public void OnGet()
        {
            auth = Request.Query["auth"];

        }
    }

  
}
