using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Models;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class CheckResultsModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public Room CurrentRoom { get; set; }
        public string Twitch { get; set; }
        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            Twitch = Request.Query["twitch"];
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
            CurrentRoom = LoadRoomFromJson(roomCode);
            
        }

        private Room LoadRoomFromJson(string roomCode)
        {
            var filePath = Path.Combine("Data", "Rooms", $"{roomCode}.json");
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                CurrentRoom = JsonSerializer.Deserialize<Room>(json);
                return CurrentRoom;
            }
            else
            {
                Console.WriteLine("No se encontró el archivo de la sala");
                return null;
            }
        }
    }

}
