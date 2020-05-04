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
        
//        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<User> Users { get; set; }
        
        public DbSet<News> Newses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
//            var converter = new ValueConverter<BigInteger, long>(    
//                model => (long)model,
//                provider => new BigInteger(provider));
//
//            modelBuilder
//                .Entity<User>()
//                .Property(e => e.Id)
//                .HasConversion(converter);
        }
    }
}