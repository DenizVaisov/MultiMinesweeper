using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiMinesweeper.Model
{
    [Table("high_scores")]
    public class HighScores
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }
        
        [Column("points")]
        public long Points { get; set; }
        
        [Column("plus_points")]
        public int PlusRating { get; set; }
        
        [Column("minus_points")]
        public int MinusRating { get; set; }
        
        [Column("firstPlayer")]
        public string FirstPlayer { get; set; }
        
        [Column("secondPlayer")]
        public string SecondPlayer { get; set; }
        
        [Column("win")]
        public string Win { get; set; }
        
        [Column("lose")]
        public string Lose { get; set; }
    }
}