using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using QuizClash_Arena_Multimedia.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace QuizClash_Arena_Multimedia.Pages
{
    public class upload_memesModel : PageModel
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IHostApplicationLifetime _applicationLifetime;
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public List<string> UploadedMemes { get; set; } = new List<string>();


        [BindProperty]
        public List<IFormFile> Files { get; set; }

        public int NumPlayers { get; set; }

        public upload_memesModel(IHubContext<GameHub> hubContext, IHostApplicationLifetime applicationLifetime)
        {
            _hubContext = hubContext;
            _applicationLifetime = applicationLifetime;
        }



        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;

            // Aquí puedes procesar los datos según sea necesario.
        }

        // Método POST para manejar la subida de archivos
        public async Task<IActionResult> OnPostAsync()
        {
            // Establece el directorio de destino
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Memes");
            try
            {
                foreach (var file in Files)
                {
                    // Obtén la ruta completa del archivo
                    var filePath = Path.Combine(uploadPath, file.FileName);

                    // Sube el archivo a la carpeta
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                // Actualiza la lista de memes subidos
                UploadedMemes = Directory.GetFiles(uploadPath).Select(f => Path.GetFileName(f)).ToList();

                // Devuelve el HTML actualizado de las imágenes subidas
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Maneja cualquier error que ocurra durante la subida
                Console.WriteLine($"Error al subir archivos: {ex.Message}");
                return RedirectToPage();  // Redirige a la misma página si algo falla
            }
        }
    }
}
