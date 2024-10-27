using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Create_gameModel : PageModel
    {
        [BindProperty]
        public int NumPlayers { get; set; }
        [BindProperty]
        public string PlayerName { get; set; }
        [BindProperty]
        public string PlayerAvatar { get; set; }

        /**
         * Maneja la solicitud POST para crear un nuevo juego.
         * Valida el estado del modelo, genera un código de sala aleatorio y redirige a la sala de espera.
         */
        public IActionResult OnPostCreateGame()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            string roomCode = GenerateRoomCode();
            return RedirectToPage("WaitingRoom", new { roomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }

        /**
         * Genera un código de sala aleatorio de 6 dígitos.
         * @return Un string que representa el código de la sala.
         */
        private string GenerateRoomCode()
        {
            return "" + new Random().Next(100000, 999999);
        }
    }
}


