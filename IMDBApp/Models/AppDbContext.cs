using Microsoft.EntityFrameworkCore;

namespace IMDBApp.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):
            base(options) { }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set;}
    }
}
