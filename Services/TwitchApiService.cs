using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

public class TwitchApiService
{
    private readonly HttpClient _httpClient;

    public TwitchApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public class OAuthTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }


    public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret)
    {
        // URL de la API de Twitch para obtener el token
        var tokenUrl = "https://id.twitch.tv/oauth2/token";

        // Parámetros necesarios para la solicitud POST
        var parameters = new Dictionary<string, string>
    {
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "grant_type", "client_credentials" }
    };

        // Crear el contenido para la solicitud POST
        var content = new FormUrlEncodedContent(parameters);

        // Realizar la solicitud POST
        var response = await _httpClient.PostAsync(tokenUrl, content);

        // Verificar si la solicitud fue exitosa
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al obtener el token de acceso: {response.StatusCode}");
        }

        // Leer la respuesta JSON que contiene el access_token
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserializar la respuesta en un modelo fuerte
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent);

        // Validar que el access_token no sea nulo
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            throw new Exception("No se pudo obtener un token válido.");
        }

        // Mostrar el token en consola (para pruebas)
        Console.WriteLine($"Access Token: {tokenResponse.access_token}");

        // Devolver el access_token como string
        return tokenResponse.access_token;
    }

    public async Task<string> GetTwitchUserNameAsync(string accessToken, string clientId)
    {
        // URL de la API de Twitch para obtener el nombre de usuario
        var url = "https://api.twitch.tv/helix/users";

        // Crear una solicitud GET con los encabezados adecuados
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", "Bearer " + accessToken);
        request.Headers.Add("Client-Id", clientId);

        // Enviar la solicitud
        var response = await _httpClient.SendAsync(request);

        // Verificar si la solicitud fue exitosa
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al obtener el nombre de usuario: {response.StatusCode}");
        }

        // Leer la respuesta JSON
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserializar la respuesta JSON para obtener el nombre de usuario
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Extraer el nombre de usuario del JSON
        var userName = jsonResponse.GetProperty("data")[0].GetProperty("login").GetString();

        // Devolver el nombre de usuario
        return userName;
    }

    public async Task<string> CreatePollAsync(string accessToken,string clientId,string broadcasterId,string title,List<string> choices,int duration,bool channelPointsVotingEnabled = false,int channelPointsPerVote = 0)
    {
        // URL del endpoint de Twitch
        var pollUrl = "https://api.twitch.tv/helix/polls";

        // Crear la carga útil (body) de la solicitud
        var pollData = new
        {
            broadcaster_id = broadcasterId,
            title = title,
            choices = choices.Select(choice => new { title = choice }).ToList(),
            duration = duration,
            channel_points_voting_enabled = channelPointsVotingEnabled,
            channel_points_per_vote = channelPointsPerVote
        };

        // Serializar los datos a JSON
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(pollData),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        // Configurar los encabezados de la solicitud
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        _httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);

        // Enviar la solicitud POST
        var response = await _httpClient.PostAsync(pollUrl, content);

        // Verificar el resultado de la solicitud
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear la encuesta: {response.StatusCode} - {errorContent}");
        }

        // Leer y devolver la respuesta como string
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
    public async Task<string> UpdatePollStatusAsync(
    string accessToken,
    string clientId,
    string broadcasterId,
    string pollId,
    string status)
    {
        // URL del endpoint de Twitch con parámetros de consulta
        var url = $"https://api.twitch.tv/helix/polls?broadcaster_id={broadcasterId}&id={pollId}&status={status}";

        // Configurar los encabezados de la solicitud
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        _httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);

        // Enviar la solicitud PATCH
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(string.Empty) // Twitch no requiere un cuerpo en esta solicitud
        };

        var response = await _httpClient.SendAsync(request);

        // Verificar si la solicitud fue exitosa
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al actualizar el estado de la encuesta: {response.StatusCode} - {errorContent}");
        }

        // Leer y devolver la respuesta como string
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }


}
