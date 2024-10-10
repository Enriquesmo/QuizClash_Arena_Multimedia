using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Create_gameModel : PageModel
    {
        public string RoomCode { get; private set; } // Código de la sala

        public void OnGet()
        {
            // No es necesario generar un código aquí
        }

        public IActionResult OnPost(int numPlayers)
        {
            // Generar un código de sala aleatorio de 6 dígitos
            Random random = new Random();
            RoomCode = random.Next(100000, 999999).ToString();

            // Redirigir a la sala de espera con el código de la sala y número de jugadores
            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode, numPlayers });
        }
    }
}
