using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Login_TwitchModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirige al usuario a Twitch para autenticarse
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Twitch");
        }
    }
}
