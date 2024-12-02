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

            // Al registrar el evento, se eliminarán los archivos al detenerse la aplicación
            _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        }

        // Método para eliminar archivos cuando la aplicación se detiene
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
                        System.IO.File.Delete(file);  // Asegúrate de usar System.IO.File
                    }
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error durante la eliminación
                    Console.WriteLine($"Error al eliminar archivos: {ex.Message}");
                }
            }
        }

        // Captura el número de jugadores al cargar la página
        public void OnGet(int numPlayers)
        {
            NumPlayers = numPlayers;
        }

        // Método POST para manejar la subida de archivos
        public async Task<IActionResult> OnPostAsync()
        {
            if (Files == null || Files.Count == 0)
            {
                // Si no se seleccionaron archivos, no se hace nada
                return Partial("_UploadedMemes", UploadedMemes); // Devuelve el HTML actualizado de las imágenes subidas
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
                return Partial("_UploadedMemes", UploadedMemes);
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
