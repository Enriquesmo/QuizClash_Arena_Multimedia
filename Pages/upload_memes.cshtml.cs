using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR; // Asegúrate de incluir esta línea
using QuizClash_Arena_Multimedia.Hubs; // Asegúrate de incluir el namespace de tu GameHub
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class upload_memesModel : PageModel
    {
        private readonly IHubContext<GameHub> _hubContext; // Inyectar el contexto del Hub



        [BindProperty]
        public List<IFormFile> Files { get; set; }

        public int NumPlayers { get; set; } // Propiedad para el número de jugadores

        public upload_memesModel(IHubContext<GameHub> hubContext) // Constructor
        {
            _hubContext = hubContext;
        }

        public void OnGet(int numPlayers)
        {
            NumPlayers = numPlayers; // Captura el número de jugadores al cargar la página
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Files == null || Files.Count == 0)
            {
                return new JsonResult(new { success = false, message = "No se seleccionaron archivos." });
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            try
            {
                foreach (var file in Files)
                {
                    var filePath = Path.Combine(uploadPath, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                return new JsonResult(new { success = true, message = "Archivos subidos correctamente." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error al subir archivos: {ex.Message}" });
            }
        }

    }
}
