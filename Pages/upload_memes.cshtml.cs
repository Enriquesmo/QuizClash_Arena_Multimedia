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
        public List<string> UploadedMemes { get; set; } = new List<string>();


        [BindProperty]
        public List<IFormFile> Files { get; set; }

        public int NumPlayers { get; set; }

        public upload_memesModel(IHubContext<GameHub> hubContext, IHostApplicationLifetime applicationLifetime)
        {
            _hubContext = hubContext;
            _applicationLifetime = applicationLifetime;

            // Al registrar el evento, se eliminar�n los archivos al detenerse la aplicaci�n
            _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        }

        // M�todo para eliminar archivos cuando la aplicaci�n se detiene
        private void OnApplicationStopping()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Memes");
            if (Directory.Exists(uploadPath))
            {
                try
                {
                    // Eliminar todos los archivos en el directorio
                    foreach (var file in Directory.GetFiles(uploadPath))
                    {
                        System.IO.File.Delete(file);  // Aseg�rate de usar System.IO.File
                    }
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error durante la eliminaci�n
                    Console.WriteLine($"Error al eliminar archivos: {ex.Message}");
                }
            }
        }

        // Captura el n�mero de jugadores al cargar la p�gina
        public void OnGet(int numPlayers)
        {
            NumPlayers = numPlayers;
        }

        // M�todo POST para manejar la subida de archivos
        public async Task<IActionResult> OnPostAsync()
        {
            if (Files == null || Files.Count == 0)
            {
                // Si no se seleccionaron archivos, no se hace nada
                return Partial("_UploadedMemes", UploadedMemes); // Devuelve el HTML actualizado de las im�genes subidas
            }

            // Establece el directorio de destino
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Memes");

            // Si la carpeta no existe, crea la carpeta
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            try
            {
                foreach (var file in Files)
                {
                    // Obt�n la ruta completa del archivo
                    var filePath = Path.Combine(uploadPath, file.FileName);

                    // Sube el archivo a la carpeta
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                // Actualiza la lista de memes subidos
                UploadedMemes = Directory.GetFiles(uploadPath).Select(f => Path.GetFileName(f)).ToList();

                // Devuelve el HTML actualizado de las im�genes subidas
                return Partial("_UploadedMemes", UploadedMemes);
            }
            catch (Exception ex)
            {
                // Maneja cualquier error que ocurra durante la subida
                Console.WriteLine($"Error al subir archivos: {ex.Message}");
                return RedirectToPage();  // Redirige a la misma p�gina si algo falla
            }
        }
    }
}
