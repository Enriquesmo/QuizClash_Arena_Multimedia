using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class JoinGameModel : PageModel
    {
        [BindProperty]
        public string RoomCode { get; set; }
        [BindProperty]
        public string PlayerName { get; set; }
        [BindProperty]
        public string PlayerAvatar { get; set; }

        /**
         * Maneja la solicitud POST para unirse a un juego.
         * Valida el estado del modelo y redirige a la sala de espera con los datos necesarios.
         */
        public IActionResult OnPostJoinGame()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }
    }
}

