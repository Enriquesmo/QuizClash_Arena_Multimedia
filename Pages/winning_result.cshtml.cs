using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Models;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WinningResultModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public Room CurrentRoom { get; set; }
        public bool Twitch { get; set; }
        public string CreatorName { get; private set; }
        public string? Winner { get; set; }

        public void OnGet(string? roomCode, string? playerName, string? playerAvatar, bool? Twitch)
        {
            // L�gica para obtener el ganador

            //RoomCode = roomCode;
            //PlayerName = playerName;
            //PlayerAvatar = playerAvatar;
            //CurrentRoom = LoadRoomFromJson(roomCode);
            //CreatorName = CurrentRoom.CreatedBy.Name;
            //this.Twitch = Twitch ?? false;
            Winner = Request.Query["resultWin"]; // Puedes cambiar esta l�nea para obtener datos din�micos.
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
                Console.WriteLine("No se encontr� el archivo de la sala");
                return null;
            }
        }
    }
}
