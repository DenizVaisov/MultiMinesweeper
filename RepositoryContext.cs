using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiMinesweeper.Model;

namespace MultiMinesweeper
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options)
            :base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<HighScores> HighScores { get; set; }
        public DbSet<News> Newses { get; set; }
        
        public DbSet<Records> Records { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}