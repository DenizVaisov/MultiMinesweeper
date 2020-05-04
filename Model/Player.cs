using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace MultiMinesweeper.Model
{
    public class Player
    {
        public BigInteger Id { get; set; }
        
        public string ConnectionId { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}