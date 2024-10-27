using System.Collections.Generic;

namespace QuizClash_Arena_Multimedia.Models
{
    public class Room
    {
        public string RoomCode { get; set; }
        public int PlayerLimit { get; set; }
        public List<Player> Players { get; set; } // Lista de jugadores en la sala

        /**
         * Constructor de la clase Room.
         * Inicializa las propiedades RoomCode, PlayerLimit y la lista de Players.
         */
        public Room(string roomCode, int playerLimit)
        {
            RoomCode = roomCode;
            PlayerLimit = playerLimit;
            Players = new List<Player>(); // Inicializar la lista de Player
        }
    }
}

