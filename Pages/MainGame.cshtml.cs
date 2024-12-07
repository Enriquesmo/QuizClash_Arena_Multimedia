using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Models;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class MainGameModel : PageModel
    {
        public string MemePath1 { get; private set; }
        public string MemePath2 { get; private set; }
        public string MemePath3 { get; private set; }
        public string MemePath4 { get; private set; }
        public string MemePath5 { get; private set; }
        //[BindProperty]
        public string UserText { get; set; }
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public Room CurrentRoom { get; set; }
        public static int currentRound { get; set; }

        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
            currentRound = 0;
            LoadRoomFromJson(roomCode);
            if (IsRoomCreator() && !CurrentRoom.Rounds.Any())
            {
                CreateRounds();
                SaveRoomToJson();
            }
            CurrentRoom = LoadRoomFromJson(roomCode);
            MemePath1 = CurrentRoom.Rounds[0].MemePath;
            MemePath2 = CurrentRoom.Rounds[1].MemePath;
            MemePath3 = CurrentRoom.Rounds[2].MemePath;
            MemePath4 = CurrentRoom.Rounds[3].MemePath;
            MemePath5 = CurrentRoom.Rounds[4].MemePath;
        }

        /***********************************************************************************************************/
        /**NO TOCAR*************************************************************************************************/
        /**Estos metodos son para cargar las rondas y funcinoan bien************************************************/
        /***********************************************************************************************************/
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
        private void SaveRoomToJson()
        {
            var filePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");
            var json = JsonSerializer.Serialize(CurrentRoom, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
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
        private bool IsRoomCreator()
        {
            return CurrentRoom.CreatedBy.Name == PlayerName && CurrentRoom.CreatedBy.Avatar == PlayerAvatar;
        }
    }
}
