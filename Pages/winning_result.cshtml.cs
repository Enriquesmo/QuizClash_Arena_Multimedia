using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WinningResultModel : PageModel
    {
        // Propiedad p�blica para almacenar el ganador
        public string? Winner { get; set; }

        public void OnGet()
        {
            // L�gica para obtener el ganador
            Winner = Request.Query["resultWin"]; // Puedes cambiar esta l�nea para obtener datos din�micos.
        }
    }
}
