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

        // Variables para las rondas de juego
        public List<Round> Rounds { get; set; }
        public bool GameStarted { get; set; }
        public int CurrentRound { get; set; }
        public bool GameEnded { get; set; }
        public bool Voting { get; set; }

        // Constructor sin parámetros necesario para la deserialización
        public Room()
        {
            Rounds = new List<Round>();
        }

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
            Rounds = new List<Round>();
            GameStarted = false;
            CurrentRound = 0;
            GameEnded = false;
            Voting = false;
        }
    }

    public class Round
    {
        public string MemePath { get; set; }
        public List<Respuesta> Answers { get; set; }

        public Round(string memepath)
        {
            MemePath = memepath;
            Answers = new List<Respuesta>();
        }

    }

    public class Respuesta
    {
        public string PlayerName { get; set; }
        public string Response { get; set; }
        public int votos { get; set; }
        public Respuesta(string playerName, string response)
        {
            PlayerName = playerName;
            Response = response;
        }
    }
}
