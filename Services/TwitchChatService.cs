using QuizClash_Arena_Multimedia.Models;
using System.Text.Json;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using System.Reflection;
using QuizClash_Arena_Multimedia.Models;

public class TwitchChatService
{
    private readonly TwitchClient _client;
    private readonly string _broadcasterId;
    private readonly string _clientId;
    private readonly string _accessToken;
    private readonly string _roomCode;
    private List<string> _playerNames = new List<string>();
    private Dictionary<string, int> _votes;

    public TwitchChatService(string accessToken, string broadcasterId, string clientId, string roomCode)
    {
        _accessToken = accessToken;
        _broadcasterId = broadcasterId;
        _clientId = clientId;
        _roomCode = roomCode;

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

        foreach (var playerName in _playerNames)
        {
            if (message.Contains(playerName.ToLower()))
            {
                _votes[playerName]++;
                break;
            }
        }
    }

    // Iniciar votación
    public async Task<string> StartVotingAsync()
    {
        try
        {
            var filePath = Path.Combine("Data", "Rooms", $"{_roomCode}.json");
            var jsonRoomContent = await File.ReadAllTextAsync(filePath);
            var room = JsonSerializer.Deserialize<Room>(jsonRoomContent);

            _playerNames = room?.Players.Select(p => p.Name).ToList() ?? new List<string>();
            _votes = _playerNames.ToDictionary(name => name, name => 0);

            // Enviar mensaje inicial con las opciones de votación
            string voteOptions = string.Join(" o ", _playerNames);
            if (!_client.IsConnected)
            {
                _client.Connect();
                await Task.Delay(2000); // Esperar un momento para garantizar la conexión
            }

            _client.SendMessage(_client.JoinedChannels[0], $"¡Inicia la votación! Escribe el nombre de un jugador ({voteOptions}) para votar.");

            // Esperar 30 segundos
            await Task.Delay(30000);

            // Finalizar votación y enviar resultados
            var voteResultMessage = "Votación finalizada: ";
            foreach (var playerName in _playerNames)
            {
                voteResultMessage += $"{playerName} = {_votes[playerName]} ";
            }

            _client.SendMessage(_client.JoinedChannels[0], voteResultMessage);

            // Determinar el ganador
            var maxVotes = _votes.Values.Max();
            var winners = _votes.Where(v => v.Value == maxVotes).Select(v => v.Key).ToList();
            string winnerMessage;

            if (winners.Count > 1)
            {
                winnerMessage = $"Empate entre: {string.Join(", ", winners)}";
            }
            else
            {
                winnerMessage = $"Ganador: {winners.First()}";
            }
            await Task.Delay(1500);
            _client.SendMessage(_client.JoinedChannels[0], winnerMessage);
            //room.ganador = winnerMessage;
            LoadRoomFromJson(_roomCode);
            CurrentRoom.ganador = winnerMessage;
            SaveRoomToJson(_roomCode);
            return winnerMessage; // Devolver el resultado
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la votación: {ex.Message}");
            return "Hubo un error al determinar el ganador.";
        }
        finally
        {
            _client.Disconnect();
        }
    }
    public Room CurrentRoom { get; set; }

    private Room LoadRoomFromJson(string roomCode)
    {
        var filePath = Path.Combine("Data", "Rooms", $"{roomCode}.json");
        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            CurrentRoom = JsonSerializer.Deserialize<Room>(json);
            return CurrentRoom;
        }
        else
        {
            Console.WriteLine("No se encontró el archivo de la sala");
            return null;
        }
    }

    private void SaveRoomToJson(string RoomCode)
    {
        Console.WriteLine("Guardando sala en archivo JSON");
        var filePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");
        var json = JsonSerializer.Serialize(CurrentRoom, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(filePath, json);
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
