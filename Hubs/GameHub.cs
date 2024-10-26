using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Collections.Concurrent;

namespace QuizClash_Arena_Multimedia.Hubs
{
    public class GameHub : Hub
    {
        private static TwitchClient twitchClient;
        private static Dictionary<string, List<string>> playerMemes = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
        private static Dictionary<string, int> voteCount = new Dictionary<string, int>(); // Diccionario para contar los votos
        private static int currentRound = 0;
        private static Dictionary<string, List<string>> gameRooms = new Dictionary<string, List<string>>(); // Diccionario para almacenar jugadores por sala

        public GameHub()
        {
            if (twitchClient == null)
            {
                InitializeTwitchClient();
            }
        }

        private void InitializeTwitchClient()
        {
            var credentials = new ConnectionCredentials("twitch_username", "access_token");
            twitchClient = new TwitchClient();
            twitchClient.Initialize(credentials, "channel_name");

            twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
            twitchClient.Connect();
        }

        private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string message = e.ChatMessage.Message.ToLower();

            // Procesar el mensaje como voto
            if (message.StartsWith("!voto "))
            {
                string vote = message.Split(' ')[1];

                // Registrar el voto en el diccionario
                if (voteCount.ContainsKey(vote))
                {
                    voteCount[vote]++;
                }
                else
                {
                    voteCount[vote] = 1;
                }

                // Enviar el voto a todos los clientes conectados mediante SignalR
                Clients.All.SendAsync("ReceiveVote", vote, voteCount[vote]);
            }
        }

        public async Task StartGame(int numPlayers)
        {
            currentRound = 1;
            playerScores.Clear();
            voteCount.Clear(); // Limpiar los votos al iniciar un nuevo juego
            await Clients.All.SendAsync("GameStarted", numPlayers);
        }

        public async Task UploadMeme(string playerName, string memeUrl)
        {
            if (!playerMemes.ContainsKey(playerName))
            {
                playerMemes[playerName] = new List<string>();
            }
            playerMemes[playerName].Add(memeUrl);
            await Clients.All.SendAsync("MemeUploaded", playerName, memeUrl);
        }

        public async Task SubmitAnswer(string playerName, string answer)
        {
            await Clients.All.SendAsync("AnswerSubmitted", playerName, answer);
        }

        public async Task StartVoting()
        {
            voteCount.Clear(); // Limpiar los votos al iniciar una nueva ronda de votación
            await Clients.All.SendAsync("VotingStarted", currentRound);
        }

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

        private string GetWinningAnswer()
        {
            // Encuentra la respuesta con la mayor cantidad de votos
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

        // Método para unirse a la sala usando el código de la sala
        private static readonly ConcurrentDictionary<string, List<Player>> Rooms = new ConcurrentDictionary<string, List<Player>>();

        public async Task JoinRoom(string roomCode, string playerName, string playerAvatar)
        {
            // Añadir el jugador a la sala
            var player = new Player { Name = playerName, AvatarUrl = playerAvatar };
            Rooms.AddOrUpdate(roomCode, new List<Player> { player }, (key, existingList) =>
            {
                existingList.Add(player);
                return existingList;
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName, playerAvatar);
        }

        public Task<List<Player>> GetPlayersInRoom(string roomCode)
        {
            // Obtener la lista de jugadores en la sala
            Rooms.TryGetValue(roomCode, out var players);
            return Task.FromResult(players ?? new List<Player>());
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }



}

