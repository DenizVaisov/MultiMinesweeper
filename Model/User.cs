using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.AspNetCore.Identity;

namespace MultiMinesweeper.Model
{
    [Table("sessions")]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        
        [Column("email")]
        public string Email { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
        
        [Column("password")]
        public string Password { get; set; }
    }
}