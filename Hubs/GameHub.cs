using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuizClash_Arena_Multimedia.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendQuestion(string question)
        {
            await Clients.All.SendAsync("ReceiveQuestion", question);
        }

        public async Task SendAnswer(string player, string answer)
        {
            await Clients.All.SendAsync("ReceiveAnswer", player, answer);
        }
    }
}
