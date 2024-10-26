using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WaitingRoomModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }

        public void OnGet(string roomCode, string playerName, string playerAvatar)
        {
            RoomCode = roomCode;
            PlayerName = playerName;
            PlayerAvatar = playerAvatar;
        }
    }
}
