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

        public IActionResult OnPostCreateGame()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Generar un código de sala aleatorio
            string roomCode = GenerateRoomCode();

            // Redirigir a la sala de espera con los datos necesarios
            return RedirectToPage("WaitingRoom", new { roomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }

        private string GenerateRoomCode()
        {
            return "ROOM_" + new Random().Next(1000, 9999);
        }
    }
}
