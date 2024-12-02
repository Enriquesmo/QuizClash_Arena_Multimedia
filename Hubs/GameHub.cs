using Microsoft.AspNetCore.SignalR;
using QuizClash_Arena_Multimedia.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace QuizClash_Arena_Multimedia.Hubs
{
    public class GameHub : Hub
    {
        // ====================================================================
        // Variables estáticas
        // ====================================================================
        private static TwitchClient twitchClient;
        private static Dictionary<string, List<string>> playerMemes = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
        private static Dictionary<string, int> voteCount = new Dictionary<string, int>();
        private static int currentRound = 0;
        //private static readonly string RoomsDirectory = Path.Combine("wwwroot", "rooms"); // Ruta de almacenamiento de salas JSON
        private const string RoomsDirectory = "Data/Rooms";


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
        public string GetConnectionId()
        {
            return Context.ConnectionId;
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
        public async Task CreateRoom(string roomCode, int playerLimit, string creatorName, string creatorAvatar, string creatorWebSocketID)
        {
            try
            {
                var creator = new Player(creatorName, creatorAvatar, creatorWebSocketID);
                var room = new Room(roomCode, playerLimit, creator);

                // Guardar la información de la sala en un archivo JSON
                var filePath = Path.Combine(RoomsDirectory, $"{roomCode}.json");
                var json = JsonSerializer.Serialize(room, new JsonSerializerOptions { WriteIndented = true });

                if (!Directory.Exists(RoomsDirectory))
                {
                    Directory.CreateDirectory(RoomsDirectory);
                }

                await File.WriteAllTextAsync(filePath, json);
                await Clients.All.SendAsync("RoomCreated", roomCode, playerLimit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear el archivo JSON: {ex.Message}");
                throw;
            }
        }

        public async Task JoinRoom(string roomCode, string playerName, string playerAvatar)
        {
            string roomFilePath = Path.Combine(RoomsDirectory, $"{roomCode}.json");

            if (File.Exists(roomFilePath))
            {
                var roomJson = await File.ReadAllTextAsync(roomFilePath);
                var roomData = JsonSerializer.Deserialize<Room>(roomJson);

                if (!roomData.Players.Any(p => p.Name == playerName))
                {
                    var newPlayer = new Player(playerName, playerAvatar, Context.ConnectionId);
                    roomData.Players.Add(newPlayer);

                    // Guardar los cambios en el archivo JSON
                    var updatedJson = JsonSerializer.Serialize(roomData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(roomFilePath, updatedJson);

                    // Unir al jugador al grupo de SignalR
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

                    // Notificar a todos los clientes que un nuevo jugador se ha unido
                    await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName, playerAvatar);
                    //await Clients.Caller.SendAsync("RoomJoined", roomCode);
                }
            }
        }


        public async Task<List<Player>> GetPlayersInRoom(string roomCode)
        {
            var filePath = Path.Combine(RoomsDirectory, $"{roomCode}.json");

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                var room = JsonSerializer.Deserialize<Room>(json);
                return room?.Players ?? new List<Player>();
            }

            return new List<Player>();
        }

        public async Task<int> GetMaxPlayers(string roomCode)
        {
            var filePath = Path.Combine(RoomsDirectory, $"{roomCode}.json");

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                var room = JsonSerializer.Deserialize<Room>(json);
                return room?.NumPlayers ?? 0;
            }

            return 0;
        }

        public async Task<Dictionary<string, int>> GetAllRooms()
        {
            var roomDetails = new Dictionary<string, int>();
            var directoryInfo = new DirectoryInfo(RoomsDirectory);

            foreach (var file in directoryInfo.GetFiles("*.json"))
            {
                var json = await File.ReadAllTextAsync(file.FullName);
                var room = JsonSerializer.Deserialize<Room>(json);

                if (room != null)
                {
                    roomDetails.Add(room.RoomCode, room.NumPlayers);
                }
            }

            return roomDetails;
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
