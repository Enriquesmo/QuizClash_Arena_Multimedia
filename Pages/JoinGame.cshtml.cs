using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class JoinGameModel : PageModel
    {
        public string RoomCode { get; set; }

        public IActionResult OnPost()
        {
            RoomCode = Request.Form["roomCode"];

            // Redirigir a la página de espera con el código de sala
            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode });
        }
    }
}
