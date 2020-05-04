using Microsoft.AspNetCore.Identity;

namespace MultiMinesweeper.Model
{
    public class SignIn
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
    }
}