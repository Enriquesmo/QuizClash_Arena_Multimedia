namespace QuizClash_Arena_Multimedia.Models
{
    public class Player
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string WebSocketId { get; set; }
        public int votos { get; set; }

        // Constructor sin parámetros necesario para la deserialización
        public Player() { }

        /**
         * Constructor de la clase Player.
         * Inicializa las propiedades Name y Avatar.
         */
        public Player(string name, string avatar, string websocket)
        {
            Name = name;
            Avatar = avatar;
            WebSocketId = websocket;
            votos = 0;
        }
    }
}
