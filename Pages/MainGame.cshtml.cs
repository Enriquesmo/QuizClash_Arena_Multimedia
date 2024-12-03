using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Linq;

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

        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
            LoadRandomMeme();
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

        private void LoadRandomMeme()
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

            if (memes.Any())
            {
                Random random = new Random();
                var selectedMeme = memes[random.Next(memes.Count)];

                if (selectedMeme.StartsWith(memesPath))
                {
                    RandomMemePath = "/Memes/" + Path.GetFileName(selectedMeme);
                }
                else if (selectedMeme.StartsWith(repeatedMemesPath))
                {
                    RandomMemePath = "/memes_repetidos/" + Path.GetFileName(selectedMeme);
                }
            }
            else
            {
                RandomMemePath = "/images/fotos_token/1.jpeg";
            }
        }
    }
}
