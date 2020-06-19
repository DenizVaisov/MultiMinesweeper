using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Design;

namespace MultiMinesweeper.Model
{
    public class Player
    {
        public BigInteger Id { get; set; }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Lifes { get; set; }
    }
}