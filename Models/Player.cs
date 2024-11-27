namespace QuizClash_Arena_Multimedia.Models
{
    public class Player
    {
        public string name { get; set; }
        public string Avatar { get; set; }
        public string WebSocketId { get; set; }

        /**
         * Constructor de la clase Player.
         * Inicializa las propiedades Name y Avatar.
         */
        public Player(string name, string avatar, string websocket)
        {
            this.name = name;
            Avatar = avatar;
            WebSocketId = websocket;
        }
    }
}
