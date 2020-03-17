using Microsoft.EntityFrameworkCore;

namespace MultiMinesweeper
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options)
            :base(options)
        {
        }
        
        public DbSet<GameSession> GameSessions { get; set; }
        
    }
}