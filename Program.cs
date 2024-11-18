using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using QuizClash_Arena_Multimedia.Hubs;
using QuizClash_Arena_Multimedia.Services;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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

// Configurar la carpeta "uploads" como una carpeta de archivos est�ticos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Inicializar el servicio del bot de Twitch despu�s de construir la aplicaci�n
var twitchBot = app.Services.GetRequiredService<TwitchBotService>();
// twitchBot.Connect();

// Configurar el Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aqu� mapeamos el Hub para SignalR

// Configurar limpieza autom�tica de la carpeta "uploads" al detener la aplicaci�n
var hostApplicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

hostApplicationLifetime.ApplicationStopping.Register(() =>
{
    var uploadPath = Path.Combine(app.Environment.WebRootPath, "uploads");

    if (Directory.Exists(uploadPath))
    {
        foreach (var file in Directory.GetFiles(uploadPath))
        {
            try
            {
                File.Delete(file); // Eliminar cada archivo en la carpeta
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el archivo {file}: {ex.Message}");
            }
        }
    }
});

// Crear la carpeta "uploads" si no existe al iniciar la aplicaci�n
var uploadDirectory = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadDirectory))
{
    Directory.CreateDirectory(uploadDirectory);
}

// Ejecutar la aplicaci�n
app.Run();
