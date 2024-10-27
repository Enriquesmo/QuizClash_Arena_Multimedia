using Microsoft.AspNetCore.SignalR;
using QuizClash_Arena_Multimedia.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace QuizClash_Arena_Multimedia.Hubs
{
    public class GameHub : Hub
    {
        // ====================================================================
        // Variables estáticos
        // ====================================================================
        private static TwitchClient twitchClient;
        private static Dictionary<string, List<string>> playerMemes = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
        private static Dictionary<string, int> voteCount = new Dictionary<string, int>(); /*** Diccionario para contar los votos ***/
        private static int currentRound = 0;
        private static readonly ConcurrentDictionary<string, Room> Rooms = new ConcurrentDictionary<string, Room>();

        // ====================================================================
        // Constructor
        // ====================================================================
        public GameHub()
        {
            if (twitchClient == null)
            {
                InitializeTwitchClient();
            }
        }

        // ====================================================================
        // Métodos de inicialización de la conexión con Twitch
        // ====================================================================
        /**
         * Inicializa el cliente de Twitch con las credenciales y configura el evento de recepción de mensajes.
         */
        private void InitializeTwitchClient()
        {
            var credentials = new ConnectionCredentials("twitch_username", "access_token");
            twitchClient = new TwitchClient();
            twitchClient.Initialize(credentials, "channel_name");

            twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
            twitchClient.Connect();
        }

        // ====================================================================
        // Métodos de manejo de mensajes de Twitch
        // ====================================================================
        /**
         * Maneja los mensajes recibidos de Twitch y procesa los votos.
         */
        private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string message = e.ChatMessage.Message.ToLower();

            if (message.StartsWith("!voto "))
            {
                string vote = message.Split(' ')[1];

                if (voteCount.ContainsKey(vote))
                {
                    voteCount[vote]++;
                }
                else
                {
                    voteCount[vote] = 1;
                }

                Clients.All.SendAsync("ReceiveVote", vote, voteCount[vote]);
            }
        }

        // ====================================================================
        // Métodos de gestión de salas
        // ====================================================================
        /**
         * Añade un jugador a una sala y notifica a los jugadores de la sala
         */
        public async Task JoinRoom(string roomCode, string playerName, string playerAvatar)
        {
            var player = new Player(playerName, playerAvatar);
            Rooms.AddOrUpdate(roomCode, new Room(roomCode, 10) { Players = new List<Player> { player } }, (key, existingRoom) =>
            {
                existingRoom.Players.Add(player);
                return existingRoom;
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName, playerAvatar);
        }

        /**
         * Obtiene la lista de jugadores en una sala.
         */
        public Task<List<Player>> GetPlayersInRoom(string roomCode)
        {
            Rooms.TryGetValue(roomCode, out var room);
            return Task.FromResult(room?.Players ?? new List<Player>());
        }

        /**
         * Obtiene el número máximo de jugadores en una sala.
         */
        public Task<int> GetMaxPlayers(string roomCode)
        {
            if (Rooms.TryGetValue(roomCode, out var room))
            {
                return Task.FromResult(room.PlayerLimit);
            }
            return Task.FromResult(0);
        }

        // ====================================================================
        // Métodos de gestión del juego
        // ====================================================================
        /**
         * Inicia el juego, resetea las puntuaciones y los votos, y notifica a los jugadores.
         */
        public async Task StartGame(int numPlayers)
        {
            currentRound = 1;
            playerScores.Clear();
            voteCount.Clear();
            await Clients.All.SendAsync("GameStarted", numPlayers);
        }

        /**
         * Sube un meme para un jugador y notifica a los jugadores.
         */
        public async Task UploadMeme(string playerName, string memeUrl)
        {
            if (!playerMemes.ContainsKey(playerName))
            {
                playerMemes[playerName] = new List<string>();
            }
            playerMemes[playerName].Add(memeUrl);
            await Clients.All.SendAsync("MemeUploaded", playerName, memeUrl);
        }

        /**
         * Envía una respuesta de un jugador y notifica a los jugadores.
         */
        public async Task SubmitAnswer(string playerName, string answer)
        {
            await Clients.All.SendAsync("AnswerSubmitted", playerName, answer);
        }

        /**
         * Inicia una nueva ronda de votación y notifica a los jugadoers.
         */
        public async Task StartVoting()
        {
            voteCount.Clear();
            await Clients.All.SendAsync("VotingStarted", currentRound);
        }

        /**
         * Calcula el resultado de la votación y actualiza las puntuaciones de los jugadores.
         */
        public async Task VoteResult()
        {
            string winningAnswer = GetWinningAnswer();

            if (!string.IsNullOrEmpty(winningAnswer))
            {
                if (playerScores.ContainsKey(winningAnswer))
                {
                    playerScores[winningAnswer]++;
                }
                else
                {
                    playerScores[winningAnswer] = 1;
                }

                if (playerScores[winningAnswer] >= 3)
                {
                    await Clients.All.SendAsync("GameEnded", winningAnswer);
                }
                else
                {
                    currentRound++;
                    await Clients.All.SendAsync("NewRoundStarted", currentRound);
                }
            }
            else
            {
                await Clients.All.SendAsync("NoVotesReceived", "No hubo votos en esta ronda");
            }
        }

        /**
         * Encuentra la respuesta con la mayor cantidad de votos.
         */
        private string GetWinningAnswer()
        {
            int maxVotes = 0;
            string winningAnswer = null;
            foreach (var vote in voteCount)
            {
                if (vote.Value > maxVotes)
                {
                    maxVotes = vote.Value;
                    winningAnswer = vote.Key;
                }
            }
            return winningAnswer;
        }
    }
}
