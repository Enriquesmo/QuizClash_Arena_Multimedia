using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Agregar Razor Pages y SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();  // Agregar SignalR

// Configuraci�n de autenticaci�n para Twitch
builder.Services.AddAuthentication(options =>
{
    // Especificar el esquema de autenticaci�n predeterminado y el esquema de desaf�o
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";
})
.AddCookie()  // Agregar autenticaci�n basada en cookies
.AddTwitch(options =>
{
    // Configurar la autenticaci�n de Twitch con ClientId y ClientSecret
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
    // Configurar el manejo de excepciones y HSTS para entornos de producci�n
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configurar middleware para redirecci�n HTTPS y servir archivos est�ticos
app.UseHttpsRedirection();
app.UseStaticFiles(); // Aseg�rate de que esto est� habilitado
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Servir archivos de la carpeta "avatars"
app.UseStaticFiles(new StaticFileOptions
{
    // Configurar el proveedor de archivos f�sicos para la carpeta "avatars"
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images/avatars")),
    RequestPath = "/images/avatars"
});

// Inicializar el servicio del bot de Twitch despu�s de construir la aplicaci�n
var twitchBot = app.Services.GetRequiredService<TwitchBotService>();
//twitchBot.Connect();

// Configurar el Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aqu� mapeamos el Hub para SignalR

// Ejecutar la aplicaci�n
app.Run();
