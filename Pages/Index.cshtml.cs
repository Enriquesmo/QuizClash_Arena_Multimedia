using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        /**
         * Constructor de la clase IndexModel.
         * Inicializa el logger.
         * @param logger El logger inyectado.
         */
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        /**
         * Maneja las solicitudes GET a la p�gina.
         * Actualmente no realiza ninguna acci�n espec�fica.
         */
        public void OnGet()
        {

        }
    }
}



