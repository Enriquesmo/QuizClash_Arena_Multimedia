using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Models;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class MainGameModel : PageModel
    {
        public string RandomMemePath { get; private set; }
        [BindProperty]
        public string UserText { get; set; } // Propiedad para almacenar el texto ingresado por el usuario
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public Room CurrentRoom { get; set; }

        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
            LoadRoomFromJson(roomCode);

            if (IsRoomCreator() && !CurrentRoom.Rounds.Any())
            {
                CreateRounds();
                SaveRoomToJson();
            }
            // Puedes procesar estos valores o utilizarlos en la vista.
        }

        public void OnPost()
        {
            // Aquí puedes manejar el texto ingresado por el usuario
            // Puedes guardarlo, mostrarlo o realizar alguna otra acción con él
            if (!string.IsNullOrEmpty(UserText))
            {
                // Puedes imprimir el texto o procesarlo de alguna manera
                // Por ejemplo, mostrarlo en la consola
                Console.WriteLine("Texto ingresado por el usuario: " + UserText);
            }

            // Volver a cargar un meme aleatorio después de enviar el formulario
            LoadRandomMeme();
        }

        private void LoadRoomFromJson(string roomCode)
        {
            var filePath = Path.Combine("Data", "Rooms", $"{roomCode}.json");
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                CurrentRoom = JsonSerializer.Deserialize<Room>(json);
            }
            else
            {
                CurrentRoom = new Room(roomCode, 1, new Player { Name = PlayerName, Avatar = PlayerAvatar });
            }
        }

        private void SaveRoomToJson()
        {
            var filePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");
            var json = JsonSerializer.Serialize(CurrentRoom, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }

        private void CreateRounds()
        {
            var memes = LoadMemes();
            Random random = new Random();

            for (int i = 0; i < 5; i++)
            {
                if (memes.Any())
                {
                    var selectedMeme = memes[random.Next(memes.Count)];
                    var memePath = selectedMeme.StartsWith("wwwroot/Memes") ? "/Memes/" + Path.GetFileName(selectedMeme) : "/memes_repetidos/" + Path.GetFileName(selectedMeme);
                    CurrentRoom.Rounds.Add(new Round(memePath));
                }
            }
        }

        private List<string> LoadMemes()
        {
            var memesPath = Path.Combine("wwwroot", "Memes");
            var repeatedMemesPath = Path.Combine("wwwroot", "memes_repetidos");
            var memes = new List<string>();

            if (Directory.Exists(memesPath))
            {
                memes.AddRange(Directory.GetFiles(memesPath, "*.png"));
                memes.AddRange(Directory.GetFiles(memesPath, "*.jpeg"));
                memes.AddRange(Directory.GetFiles(memesPath, "*.jpg"));
            }

            if (Directory.Exists(repeatedMemesPath))
            {
                memes.AddRange(Directory.GetFiles(repeatedMemesPath, "*.png"));
                memes.AddRange(Directory.GetFiles(repeatedMemesPath, "*.jpeg"));
                memes.AddRange(Directory.GetFiles(repeatedMemesPath, "*.jpg"));
            }

            return memes;
        }

        private void LoadRandomMeme()
        {
            var memes = LoadMemes();

            if (memes.Any())
            {
                Random random = new Random();
                var selectedMeme = memes[random.Next(memes.Count)];

                if (selectedMeme.StartsWith("wwwroot/Memes"))
                {
                    RandomMemePath = "/Memes/" + Path.GetFileName(selectedMeme);
                }
                else if (selectedMeme.StartsWith("wwwroot/memes_repetidos"))
                {
                    RandomMemePath = "/memes_repetidos/" + Path.GetFileName(selectedMeme);
                }
            }
            else
            {
                RandomMemePath = "/images/fotos_token/1.jpeg";
            }
        }

        private bool IsRoomCreator()
        {
            return CurrentRoom.CreatedBy.Name == PlayerName && CurrentRoom.CreatedBy.Avatar == PlayerAvatar;
        }
    }
}
