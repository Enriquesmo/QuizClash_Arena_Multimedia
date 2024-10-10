using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using QuizClash_Arena_Multimedia.Hubs;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WaitingRoomModel : PageModel
    {
        public string RoomCode { get; set; }
        public List<string> Players { get; private set; } = new List<string>();

        private readonly IHubContext<GameHub> _hubContext;

        public WaitingRoomModel(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void OnGet(string roomCode)
        {
            RoomCode = roomCode;
        }

        public void AddPlayer(string playerName)
        {
            Players.Add(playerName);
            // Notifica a todos los jugadores que un nuevo jugador se ha unido
            _hubContext.Clients.All.SendAsync("PlayerJoined", playerName);
        }
    }
}
