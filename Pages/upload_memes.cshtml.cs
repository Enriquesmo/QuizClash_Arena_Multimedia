using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class upload_memesModel : PageModel
    {
        [BindProperty]
        public List<IFormFile> Files { get; set; }

        public int NumPlayers { get; set; } // Propiedad para el n�mero de jugadores

        public void OnGet(int numPlayers)
        {
            NumPlayers = numPlayers; // Captura el n�mero de jugadores al cargar la p�gina
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Files != null && Files.Count > 0)
            {
                foreach (var file in Files)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                return RedirectToPage("Index"); // Redirige a la p�gina principal despu�s de subir los archivos
            }

            return Page();
        }
    }
}
