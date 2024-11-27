using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using QuizClash_Arena_Multimedia.Models;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class CreateGameModel : PageModel
    {
        [BindProperty]
        public string RoomCode { get; set; }
        [BindProperty]
        public int MaxPlayers { get; set; }
        [BindProperty]
        public string PlayerName { get; set; }
        [BindProperty]
        public string PlayerAvatar { get; set; }
        [BindProperty]
        public string WebSocketId { get; set; } // Nuevo campo para el WebSocketId

        public IActionResult OnPost()
        {
            // Generar un RoomCode de 6 cifras aleatorias
            RoomCode = new Random().Next(100000, 999999).ToString();

            // Crear el jugador que crea la sala
            var creator = new Player(PlayerName, PlayerAvatar, WebSocketId);

            // Crear un objeto para representar la sala utilizando el constructor
            var room = new Room(RoomCode, MaxPlayers, creator);

            // Añadir el creador a la lista de jugadores
            room.Players.Add(creator);

            // Ruta para guardar el archivo JSON de la sala
            string roomFilePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");

            // Crear la carpeta si no existe
            Directory.CreateDirectory(Path.GetDirectoryName(roomFilePath));

            // Guardar el archivo JSON
            string roomJson = JsonSerializer.Serialize(room, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(roomFilePath, roomJson);

            // Redirigir a la sala de espera con los datos del creador
            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }
    }
}
