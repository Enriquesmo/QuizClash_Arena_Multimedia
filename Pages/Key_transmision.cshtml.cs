using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Services;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class Key_transmisionModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly TwitchApiService _twitchApiService;

        public Key_transmisionModel(IHttpClientFactory clientFactory, TwitchApiService twitchApiService)
        {
            _clientFactory = clientFactory;
            _twitchApiService = twitchApiService;
        }
        public string ProcessStreamKey(string streamKey, string broadcasterId)
        {
            // Aquí puedes implementar lógica adicional, como guardar el streamKey en un log,
            // mostrarlo en algún formato específico, etc.
            return $"Broadcaster ID: {broadcasterId}, Stream Key: {streamKey}";
        }
        public string? StreamKey { get; private set; }

        public async Task OnGetAsync(string? streamKey, string? broadcasterId)
        {
            if (!string.IsNullOrEmpty(streamKey) && !string.IsNullOrEmpty(broadcasterId))
            {
                StreamKey = ProcessStreamKey(streamKey, broadcasterId);
            }
            else
            {
                var accessToken = Request.Cookies["access_token"];
                broadcasterId = Request.Cookies["brodcasterId"];

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(broadcasterId))
                {
                    try
                    {
                        streamKey = await _twitchApiService.GetStreamKeyAsync(
                            accessToken,
                            HttpContext.RequestServices.GetService<IConfiguration>()?["Twitch:ClientId"],
                            broadcasterId
                        );

                        StreamKey = ProcessStreamKey(streamKey, broadcasterId);
                    }
                    catch (Exception ex)
                    {
                        StreamKey = $"Error: {ex.Message}";
                    }
                }
                else
                {
                    StreamKey = "No se encontraron datos para procesar la clave de transmisión.";
                }
            }
        }

    }
}
