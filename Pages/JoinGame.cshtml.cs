using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using QuizClash_Arena_Multimedia.Models;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class JoinGameModel : PageModel
    {
        [BindProperty]
        public string RoomCode { get; set; }
        [BindProperty]
        public string PlayerName { get; set; }
        [BindProperty]
        public string PlayerAvatar { get; set; }
        [BindProperty]
        public string WebSocketId { get; set; }
        public Room CurrentRoom { get; set; }
        
        /**
         * Maneja la solicitud POST para unirse a un juego.
         * Valida el estado del modelo y redirige a la sala de espera con los datos necesarios.
         */
        public IActionResult OnPostJoinGame()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ruta del archivo JSON de la sala
            string roomFilePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");

            if (!System.IO.File.Exists(roomFilePath))
            {
                ModelState.AddModelError(string.Empty, "La sala no existe.");
                return Page();
            }

            // Leer el archivo JSON de la sala
            string roomJson = System.IO.File.ReadAllText(roomFilePath);
            using (JsonDocument document = JsonDocument.Parse(roomJson))
            {
                JsonElement root = document.RootElement;
                int numPlayers = root.GetProperty("NumPlayers").GetInt32();
                var playersJSON = root.GetProperty("Players");
                if (playersJSON.GetArrayLength() >= numPlayers)
                {
                    ModelState.AddModelError(string.Empty, "La sala ya está llena.");
                    return Page();
                }
                // Crear el nuevo jugador como un JsonObject
                var newPlayer = new JsonObject
                {
                    ["Name"] = PlayerName,
                    ["Avatar"] = PlayerAvatar,
                    ["WebSocketId"] = WebSocketId
                };

                // Convertir el JsonElement a JsonNode para modificarlo
                var roomNode = JsonNode.Parse(root.GetRawText());

                // Añadir el nuevo jugador a la lista de jugadores
                var playersArray = roomNode["Players"].AsArray();
                playersArray.Add(newPlayer);

                // Guardar el archivo JSON actualizado
                roomJson = roomNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(roomFilePath, roomJson);
            }

            // Redirigir a la sala de espera con los datos del nuevo jugador

            return RedirectToPage("WaitingRoom", new { roomCode = RoomCode, playerName = PlayerName, playerAvatar = PlayerAvatar });
        }

    }
}
