using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WaitingRoomModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }

        /**
         * Método que se ejecuta cuando se realiza una solicitud GET a la página.
         * Inicializa las propiedades RoomCode, PlayerName y PlayerAvatar con los valores proporcionados, 
         * ya sea del creador de la Sala o de un jugador nuevo.
         */
        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
        }
    }
}
