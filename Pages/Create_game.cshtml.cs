using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using QuizClash_Arena_Multimedia.Hubs;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Create_gameModel : PageModel
    {
        private readonly IHubContext<GameHub> _hubContext;

        public Create_gameModel(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [BindProperty]
        public int NumPlayers { get; set; }

        [BindProperty]
        public string PlayerName { get; set; }

        [BindProperty]
        public string PlayerAvatar { get; set; }

        public async Task<IActionResult> OnPostCreateGameAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Generar un código de sala único
            var roomCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Crear la sala y guardarla en el GameHub
            await _hubContext.Clients.All.SendAsync("CreateRoom", roomCode, NumPlayers);

            // Redirigir a la sala de espera
            return RedirectToPage("WaitingRoom", new { roomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }
    }
}
