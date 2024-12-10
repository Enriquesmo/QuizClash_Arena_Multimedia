using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizClash_Arena_Multimedia.Models;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QuizClash_Arena_Multimedia.Pages
{
    public class WinningResultModel : PageModel
    {
        public string RoomCode { get; set; }
        public string PlayerName { get; set; }
        public string PlayerAvatar { get; set; }
        public Room CurrentRoom { get; set; }
        public bool Twitch { get; set; }
        public string CreatorName { get; private set; }
        public string? Winner { get; set; }



            public void OnGet(string? roomCode, string? playerName, string? playerAvatar, bool? Twitch)
            {
                Winner = Request.Query["resultWin"];
                RoomCode = Request.Query["roomCode"];

                if (RoomCode != null)
                {
                    CurrentRoom = LoadRoomFromJson(RoomCode);
                    if (CurrentRoom != null)
                    {
                        if (!string.IsNullOrEmpty(Winner))
                        {
                            // Caso 1: Guardar resultWin en ganador
                            CurrentRoom.ganador = Winner;
                            SaveRoomToJson();
                        }
                        else if (!string.IsNullOrEmpty(CurrentRoom.ganador))
                        {
                            // Caso 2: Consultar CurrentRoom.ganador
                            Winner = CurrentRoom.ganador;
                        }
                        else
                        {
                            // Caso 3 y 4: Comparar puntuaciones y determinar ganador o empate
                            var players = CurrentRoom.Players;
                            if (players != null && players.Count > 0)
                            {
                                var maxVotes = players.Max(p => p.votos);
                                var winners = players.Where(p => p.votos == maxVotes).ToList();

                                if (winners.Count == 1)
                                {
                                    Winner = winners.First().Name;
                                }
                                else
                                {
                                    Winner = "Empate entre: " + string.Join(", ", winners.Select(p => p.Name));
                                }
                            }
                        }
                    }
                }

                Console.WriteLine(Winner);
            }

            private Room LoadRoomFromJson(string roomCode)
            {
                var filePath = Path.Combine("Data", "Rooms", $"{roomCode}.json");
                if (System.IO.File.Exists(filePath))
                {
                    var json = System.IO.File.ReadAllText(filePath);
                    CurrentRoom = JsonSerializer.Deserialize<Room>(json);
                    return CurrentRoom;
                }
                else
                {
                    Console.WriteLine("No se encontró el archivo de la sala");
                    return null;
                }
            }

            private void SaveRoomToJson()
            {
                var filePath = Path.Combine("Data", "Rooms", $"{RoomCode}.json");
                var json = JsonSerializer.Serialize(CurrentRoom, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);
            }
        }
    }



