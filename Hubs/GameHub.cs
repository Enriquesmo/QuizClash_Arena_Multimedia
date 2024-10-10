using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;

namespace QuizClash_Arena_Multimedia.Hubs
{
    public class GameHub : Hub
    {
        private static TwitchClient twitchClient;
        private static Dictionary<string, List<string>> playerMemes = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
        private static int currentRound = 0;

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
            // Asumiremos que los votos son mensajes tipo "!voto 1", "!voto 2", etc.
            if (message.StartsWith("!voto "))
            {
                string vote = message.Split(' ')[1];
                Clients.All.SendAsync("ReceiveVote", vote);
            }
        }

        public async Task StartGame(int numPlayers)
        {
            currentRound = 1;
            playerScores.Clear();
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
            await Clients.All.SendAsync("VotingStarted", currentRound);
        }

        public async Task VoteResult(string winningAnswer)
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
    }
}
