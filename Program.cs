using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// Configuraci�n de autenticaci�n para Twitch
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";
})
.AddCookie()
.AddTwitch(options =>
{
    options.ClientId = builder.Configuration["Twitch:ClientId"];
    options.ClientSecret = builder.Configuration["Twitch:ClientSecret"];
    options.CallbackPath = new PathString("/auth/twitch/callback");
});

// Registrar el servicio para el bot de Twitch antes de construir la aplicaci�n
builder.Services.AddSingleton<TwitchBotService>();

// Construir la aplicaci�n
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Inicializar el servicio del bot de Twitch despu�s de construir la aplicaci�n
var twitchBot = app.Services.GetRequiredService<TwitchBotService>();
twitchBot.Connect();

// Configurar el Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aqu� mapeamos el Hub para SignalR

app.Run();
