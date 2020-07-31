using Microsoft.CSharp.RuntimeBinder;

namespace MultiMinesweeper.Model
{
    public class Register
    {
        
        public string Login { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; } 
        public string Message { get; set; }
    }
}