using Game.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Database
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=game.db");
            }
        }
    }
}
