using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class TwitchChatService
{
    private readonly TwitchClient _client;
    private int _votesJ1 = 0;
    private int _votesJ2 = 0;
    private readonly string _broadcasterId;
    private readonly string _clientId;
    private readonly string _accessToken;

    public TwitchChatService(string accessToken, string broadcasterId, string clientId)
    {
        _accessToken = accessToken;
        _broadcasterId = broadcasterId;
        _clientId = clientId;

        // Obtener el nombre de usuario usando el broadcasterId
        var username = GetUsernameFromBroadcasterId(broadcasterId, accessToken, clientId).Result;

        // Configurar credenciales y cliente
        var credentials = new ConnectionCredentials(username, accessToken);
        _client = new TwitchClient();
        _client.Initialize(credentials, username);

        // Suscribirse al evento de recibir mensajes
        _client.OnMessageReceived += Client_OnMessageReceived;
    }

    // Evento de mensaje recibido
    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        var message = e.ChatMessage.Message.ToLower();
        if (message.Contains("j1")) _votesJ1++;
        else if (message.Contains("j2")) _votesJ2++;
    }

    // Iniciar votación
    public async Task StartVotingAsync()
    {
        try
        {
            _client.Connect();
            _client.SendMessage(_client.JoinedChannels[0], "¡Inicia la votación! Escribe 'j1' o 'j2' para votar.");

            // Esperar 30 segundos
            await Task.Delay(30000);

            // Finalizar votación y enviar resultados
            _client.SendMessage(_client.JoinedChannels[0], $"Votación finalizada: J1 = {_votesJ1}, J2 = {_votesJ2}");

            // Determinar el ganador
            var winner = _votesJ1 > _votesJ2 ? "J1" : _votesJ1 < _votesJ2 ? "J2" : "Empate";
            _client.SendMessage(_client.JoinedChannels[0], $"Resultado: {winner}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la votación: {ex.Message}");
        }
        finally
        {
            _client.Disconnect();
        }
    }

    // Obtener nombre de usuario desde la API de Twitch
    private async Task<string> GetUsernameFromBroadcasterId(string broadcasterId, string accessToken, string clientId)
    {
        try
        {
            var url = $"https://api.twitch.tv/helix/users?id={broadcasterId}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("Client-Id", clientId);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var userData = JsonDocument.Parse(responseBody);
            return userData.RootElement.GetProperty("data")[0].GetProperty("login").GetString();
        }
        catch (Exception ex)
        {
            throw new Exception("Error obteniendo el nombre de usuario desde Twitch.", ex);
        }
    }
}
