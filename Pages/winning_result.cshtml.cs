using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WinningResultModel : PageModel
    {
        // Propiedad pública para almacenar el ganador
        public string? Winner { get; set; }

        public void OnGet()
        {
            // Lógica para obtener el ganador
            Winner = Request.Query["resultWin"]; // Puedes cambiar esta línea para obtener datos dinámicos.
        }
    }
}
