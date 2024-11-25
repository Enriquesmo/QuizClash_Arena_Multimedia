using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.FileProviders;
using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// Agregar servicios
// ====================================================================

// Registrar el servicio para Twitch (Servicio HTTP para API de Twitch)
builder.Services.AddHttpClient<TwitchApiService>();  // Esto registra el servicio para interactuar con la API de Twitch

// Configuración de autenticación con Twitch
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";  // Usamos "Twitch" como el esquema de desafío predeterminado
})
.AddCookie() // Usar cookies para almacenar la sesión de autenticación
.AddOAuth("Twitch", options =>
{
    options.ClientId = builder.Configuration["Twitch:ClientId"];
    options.ClientSecret = builder.Configuration["Twitch:ClientSecret"];
    options.CallbackPath = new PathString("/auth/callback");
    options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
    options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";
    options.Scope.Add("user:edit:broadcast");
    options.Scope.Add("channel:manage:broadcast");
    options.Scope.Add("channel:read:stream_key");

    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");  // Mapeamos el ID
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name"); // Mapeamos el nombre para User.Identity.Name
    options.ClaimActions.MapJsonKey("login", "login");               // Login (nombre corto)
    options.ClaimActions.MapJsonKey("profile_image", "profile_image_url");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // Obtenemos la información del usuario desde la API de Twitch
            var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
            request.Headers.Add("Authorization", $"Bearer {context.AccessToken}");
            request.Headers.Add("Client-Id", options.ClientId);

            var response = await context.Backchannel.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var userData = user.RootElement.GetProperty("data")[0];

            // Agregamos claims adicionales
            context.Identity.AddClaim(new Claim(ClaimTypes.Name, userData.GetProperty("display_name").GetString()));
            context.Identity.AddClaim(new Claim("brodcasterId", userData.GetProperty("id").GetString()));
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Error en OAuth: {context.Failure.Message}");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});




// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// ====================================================================
// Construir la aplicación
// ====================================================================
var app = builder.Build();

// Configuración de excepciones y HSTS para producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configuración de middleware para redirección HTTPS y servir archivos estáticos
app.UseHttpsRedirection();
app.UseStaticFiles();  // Asegúrate de que esto está habilitado

// Configurar el pipeline de autenticación y autorización
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de Razor Pages
app.MapRazorPages();

// Configuración del Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aquí mapeamos el Hub para SignalR

// ====================================================================
// Endpoints
// ====================================================================

app.MapGet("/prueba", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    context.Response.Cookies.Delete("access_token");
    context.Response.Cookies.Delete("brodcasterId");

    var accessToken = context.Request.Cookies["access_token"];
    var broadcasterId = context.Request.Cookies["brodcasterId"];

    // Verificamos si ya están presentes las cookies
    if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(broadcasterId))
    {
        return Results.Redirect("/Login_Twitch");
    }

    // Si no, procedemos con la autenticación
    var authenticateResult = await context.AuthenticateAsync("Twitch");

    if (!authenticateResult.Succeeded)
    {
        return Results.BadRequest("No se pudo autenticar con Twitch.");
    }

    // Acceder al token de acceso
    accessToken = authenticateResult.Properties.GetTokenValue("access_token");
    var clientId = builder.Configuration["Twitch:ClientId"];

    if (string.IsNullOrEmpty(accessToken))
    {
        return Results.BadRequest("No se obtuvo el token de acceso.");
    }

    // Hacemos una solicitud para obtener el broadcasterId
    var httpClient = clientFactory.CreateClient();
    var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
    requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
    requestMessage.Headers.Add("Client-Id", clientId);

    var response = await httpClient.SendAsync(requestMessage);
    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest("No se pudo obtener la información del usuario desde Twitch.");
    }

    var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
    broadcasterId = userInfo?.Data?.FirstOrDefault()?.Id;

    if (string.IsNullOrEmpty(broadcasterId))
    {
        return Results.BadRequest("No se pudo obtener el broadcasterId.");
    }

    // Guardamos el access_token y broadcasterId en cookies
    context.Response.Cookies.Append("access_token", accessToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    });

    context.Response.Cookies.Append("brodcasterId", broadcasterId, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(30)
    });

    return Results.Redirect("/Login_Twitch");
});

app.MapGet("/signin-twitch", async (HttpContext context) =>
{
    await context.ChallengeAsync("Twitch", new AuthenticationProperties
    {
        RedirectUri = "/prueba"
    });
});
app.MapGet("/prueba2", async (HttpContext context) =>
{
    await context.ChallengeAsync("Twitch", new AuthenticationProperties
    {
        RedirectUri = "/start-stream"
    });
});
app.MapGet("/start-stream", async (HttpContext context, IHttpClientFactory clientFactory, TwitchApiService twitchApiService) =>
{

    var accessToken = context.Request.Cookies["access_token"];
    var broadcasterId = context.Request.Cookies["brodcasterId"];

    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(broadcasterId))
    {
        return Results.BadRequest("No se encuentran los datos necesarios.");
    }

    try
    {
        // Obtener la clave de transmisión
        var streamKey = await twitchApiService.GetStreamKeyAsync(accessToken, builder.Configuration["Twitch:ClientId"], broadcasterId);

        // Aquí deberías enviar la clave de transmisión a tu cliente (como OBS)
        // Esto es solo un ejemplo de mostrar la clave.
        return Results.Ok($"La clave de transmisión es: {streamKey}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error al obtener la clave de transmisión: {ex.Message}");
    }
});


// Ejecutar la aplicación
app.Run();

public class UserInfoResponse
{
    public List<UserInfo> Data { get; set; }
}

public class UserInfo
{
    public string Id { get; set; }
    public string Login { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string ProfileImageUrl { get; set; }
}
