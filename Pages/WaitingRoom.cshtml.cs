using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using QuizClash_Arena_Multimedia.Models;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WaitingRoomModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public int MaxPlayers { get; set; }
        public List<Player> playerList { get; set; }
        public Room CurrentRoom { get; set; }
        public string Twitch { get; set; }

        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            Twitch = Request.Query["twitch"];
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;

            // Ruta del archivo JSON de la sala
            string roomFilePath = Path.Combine("Data", "Rooms", $"{roomCode}.json");

            // Leer el archivo JSON y obtener los jugadores
            if (System.IO.File.Exists(roomFilePath))
            {
                string roomJson = System.IO.File.ReadAllText(roomFilePath);
                var roomData = JsonSerializer.Deserialize<Room>(roomJson);
                MaxPlayers = roomData?.NumPlayers ?? 0;
                playerList = roomData?.Players ?? new List<Player>();
                CurrentRoom = roomData;
            }
            else
            {
                MaxPlayers = 0;
                playerList = new List<Player>();
                CurrentRoom = new Room(roomCode, 1, new Player { Name = PlayerName, Avatar = PlayerAvatar });
            }
        }

        private bool IsRoomCreator()
        {
            return CurrentRoom.CreatedBy.Name == PlayerName && CurrentRoom.CreatedBy.Avatar == PlayerAvatar;
        }
    }
}
