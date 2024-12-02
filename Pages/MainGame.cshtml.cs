using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.IO;
using QuizClash_Arena_Multimedia.Models;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class MainGameModel : PageModel
    {
        public Player CurrentPlayer { get; private set; }
        public string RandomMemePath { get; private set; }

        public void OnGet(string playerId, string roomCode)
        {
            string roomFilePath = Path.Combine("data", "rooms", $"{roomCode}.json");
            if (!System.IO.File.Exists(roomFilePath))
            {
                throw new FileNotFoundException("La sala no existe.");
            }

            string roomJson = System.IO.File.ReadAllText(roomFilePath);
            Room room = JsonConvert.DeserializeObject<Room>(roomJson);

            CurrentPlayer = room.Players.FirstOrDefault(p => p.WebSocketId == playerId) ?? room.CreatedBy;

            Random random = new Random();
            var memes = Directory.GetFiles("data/Memes", "*.png")
                                 .Concat(Directory.GetFiles("data/memes_repetidos", "*.png"))
                                 .ToList();

            if (memes.Any())
            {
                RandomMemePath = memes[random.Next(memes.Count)];
            }
            else
            {
                RandomMemePath = "/images/default.png"; // Ruta de una imagen predeterminada
            }
        }

        public IActionResult OnPost(string userText, string playerId, string roomCode)
        {
            // Aquí puedes manejar el texto ingresado por el usuario
            // Ejemplo: Guardarlo en una base de datos o procesarlo.
            return RedirectToPage("/MainGame", new { playerId, roomCode });
        }
    }
}
