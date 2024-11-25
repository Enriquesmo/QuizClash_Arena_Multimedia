using System.Text.Json;

public class TwitchApiService
{
    private readonly HttpClient _httpClient;

    public TwitchApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetStreamKeyAsync(string accessToken, string clientId, string broadcasterId)
    {
        var url = $"https://api.twitch.tv/helix/streams/key?broadcaster_id={broadcasterId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", "Bearer " + accessToken);
        request.Headers.Add("Client-Id", clientId);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al obtener la clave de transmisión: {response.StatusCode}. Detalles: {content}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        var streamKey = jsonResponse.GetProperty("data")[0].GetProperty("stream_key").GetString();

        return streamKey;
    }
}
