using System;
using System.Collections.Generic;

namespace QuizClash_Arena_Multimedia.Models
{
    public class Room
    {
        public string RoomCode { get; set; }
        public int NumPlayers { get; set; }
        public Player CreatedBy { get; set; }
        public List<Player> Players { get; set; }
        public DateTime CreatedAt { get; set; }

        /**
         * Constructor de la clase Room.
         * Inicializa las propiedades RoomCode, NumPlayers, CreatedBy, Players y CreatedAt.
         */
        public Room(string roomCode, int numPlayers, Player createdBy)
        {
            RoomCode = roomCode;
            NumPlayers = numPlayers;
            CreatedBy = createdBy;
            Players = new List<Player>();
            CreatedAt = DateTime.Now;
        }
    }
}
