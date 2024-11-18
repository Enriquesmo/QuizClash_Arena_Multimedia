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

// Configuración de autenticación para Twitch
builder.Services.AddAuthentication(options =>
{
    // Especificar el esquema de autenticación predeterminado y el esquema de desafío
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Twitch";
})
.AddCookie()  // Agregar autenticación basada en cookies
.AddTwitch(options =>
{
    // Configurar la autenticación de Twitch con ClientId y ClientSecret
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
    // Configurar el manejo de excepciones y HSTS para entornos de producción
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configurar middleware para redirección HTTPS y servir archivos estáticos
app.UseHttpsRedirection();
app.UseStaticFiles(); // Asegúrate de que esto está habilitado

// Configurar la carpeta "uploads" como una carpeta de archivos estáticos
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Inicializar el servicio del bot de Twitch después de construir la aplicación
var twitchBot = app.Services.GetRequiredService<TwitchBotService>();
// twitchBot.Connect();

// Configurar el Hub de SignalR
app.MapHub<GameHub>("/gameHub");  // Aquí mapeamos el Hub para SignalR

// Configurar limpieza automática de la carpeta "uploads" al detener la aplicación
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

// Crear la carpeta "uploads" si no existe al iniciar la aplicación
var uploadDirectory = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadDirectory))
{
    Directory.CreateDirectory(uploadDirectory);
}

// Ejecutar la aplicación
app.Run();
