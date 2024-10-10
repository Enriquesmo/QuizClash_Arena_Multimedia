using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuizClash_Arena_Multimedia.Services
{
    public class TwitchBotService
    {
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchBotService> _logger;

        public TwitchBotService(IConfiguration configuration, ILogger<TwitchBotService> logger)
        {
            _logger = logger;

            var credentials = new ConnectionCredentials(
                configuration["Twitch:BotUsername"],
                configuration["Twitch:BotAccessToken"]
            );

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, "tu_nombre_canal_twitch");

            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnConnected += Client_OnConnected;
        }

        public void Connect()
        {
            _client.Connect();
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            _logger.LogInformation($"Conectado a Twitch como: {e.BotUsername}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            _logger.LogInformation($"Bot se ha unido al canal: {e.Channel}");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.StartsWith("!votar", StringComparison.InvariantCultureIgnoreCase))
            {
                var voto = e.ChatMessage.Message.Split(' ')[1];
                _logger.LogInformation($"Voto recibido: {voto} por parte de {e.ChatMessage.Username}");
                // Aquí deberías agregar lógica para contar los votos y actualizarlos en el juego.
            }
        }
    }
}
