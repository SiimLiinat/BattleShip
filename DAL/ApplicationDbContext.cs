using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Board> Boards { get; set; } = null!;
        //public DbSet<BoardState> BoardStates { get; set; } = null!;
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<GameOption> GameOptions { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<Ship> Ships { get; set; } = null!;
        public DbSet<ShipCoordinate> ShipCoordinates { get; set; } = null!;
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}