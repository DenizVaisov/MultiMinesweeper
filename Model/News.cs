using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiMinesweeper.Model
{
    [Table("news")]
    public class News 
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("title")]
        public string Title { get; set; }
        
        [Column("description")]
        public string Description { get; set; }
        
        [Column("image")]
        public string Image { get; set; }
    }
}