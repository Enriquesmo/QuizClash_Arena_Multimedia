namespace QuizClash_Arena_Multimedia.Models
{
    public class Player
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }

        /**
         * Constructor de la clase Player.
         * Inicializa las propiedades Name y AvatarUrl.
         */
        public Player(string name, string avatarUrl)
        {
            Name = name;
            AvatarUrl = avatarUrl;
        }
    }
}


