using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Create_gameModel : PageModel
    {
        public string RoomCode { get; private set; } // C�digo de la sala

        public void OnGet()
        {
            // No es necesario generar un c�digo aqu�
        }

        public IActionResult OnPost(int numPlayers)
        {
            // Generar un c�digo de sala aleatorio de 6 d�gitos
            Random random = new Random();
            RoomCode = random.Next(100000, 999999).ToString();

            // Redirigir a la sala de espera con el c�digo de la sala y n�mero de jugadores
            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode, numPlayers });
        }
    }
}
