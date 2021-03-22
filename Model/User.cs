using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiMinesweeper.Model
{
    [Table("sessions")]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        
        [Column("login")]
        public string Login { get; set; }
        
        [Column("points")]
        public double Points { get; set; }
        
        [Column("password")]
        public string Password { get; set; }
    }
}