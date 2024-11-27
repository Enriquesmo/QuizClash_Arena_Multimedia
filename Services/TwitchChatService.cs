namespace QuizClash_Arena_Multimedia.Services
{
    using TwitchLib.Client;
    using TwitchLib.Client.Models;
    using TwitchLib.Client.Events;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TwitchChatService
    {
        private readonly TwitchClient _twitchClient;
        private readonly Dictionary<string, int> _commentsCount;

        public TwitchChatService()
        {
            _commentsCount = new Dictionary<string, int>();

            var credentials = new ConnectionCredentials("your_twitch_bot_username", "your_twitch_oauth_token");
            _twitchClient = new TwitchClient();
            _twitchClient.Initialize(credentials, "your_twitch_channel_name");

            _twitchClient.OnMessageReceived += OnMessageReceived;
        }

        // Método para empezar a escuchar el chat
        public void StartListening()
        {
            _twitchClient.Connect();
        }

        // Método para detener la escucha
        public void StopListening()
        {
            _twitchClient.Disconnect();
        }

        // Evento que se dispara cuando se recibe un mensaje
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // Asegúrate de que el mensaje no sea un bot
            if (e.ChatMessage.Username != "your_twitch_bot_username")
            {
                // Filtra los mensajes de jugador1 y jugador2
                if (e.ChatMessage.Username == "jugador1" || e.ChatMessage.Username == "jugador2")
                {
                    if (_commentsCount.ContainsKey(e.ChatMessage.Message))
                    {
                        _commentsCount[e.ChatMessage.Message]++;
                    }
                    else
                    {
                        _commentsCount[e.ChatMessage.Message] = 1;
                    }
                }
            }
        }

        // Método para obtener el comentario más frecuente
        public string GetMostFrequentComment()
        {
            if (_commentsCount.Count == 0)
                return "No se recibieron comentarios.";

            var mostFrequent = _commentsCount.OrderByDescending(x => x.Value).First();
            return $"El comentario más frecuente fue: \"{mostFrequent.Key}\" con {mostFrequent.Value} menciones.";
        }
    }

}
