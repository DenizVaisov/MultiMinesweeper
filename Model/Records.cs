using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiMinesweeper.Model
{
    [Table("records")]
    public class Records
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        
        [Column("login")]
        public string Login { get; set; }
        
        [Column("points")]
        public double Points { get; set; }
        
        [Column("win")]
        public long Win { get; set; }
        
        [Column("lose")]
        public long Lose { get; set; }
    }
}