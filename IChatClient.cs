using System.Threading.Tasks;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public interface IChatClient
    {
        Task SendMessage(SignIn signIn, string message);
    }
}