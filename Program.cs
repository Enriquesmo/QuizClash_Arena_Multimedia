using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// Configuración de autenticación para Twitch
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

// Registrar el servicio para el bot de Twitch antes de construir la aplicación
builder.Services.AddSingleton<TwitchBotService>();

// Construir la aplicación
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Asegúrate de que esto está habilitado
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Servir archivos de la carpeta "avatars"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images/avatars")),
    RequestPath = "/images/avatars"
});

// Inicializar el servicio del bot de Twitch después de construir la aplicación
var twitchBot = app.Services.GetRequiredService<TwitchBotService>();
twitchBot.Connect();

// Configurar el Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aquí mapeamos el Hub para SignalR

app.Run();
