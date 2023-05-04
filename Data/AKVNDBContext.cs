using Microsoft.EntityFrameworkCore;
using AKVN_Backend.Classes;
using AKVN_Backend.Classes.DTO;


namespace AKVN_Backend.Data
{
    public class AKVNDBContext: DbContext
    {
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Scene> Scenes { get; set; }
        public DbSet<Background> Backgrounds { get; set; }
        public AKVNDBContext(DbContextOptions<AKVNDBContext> options) : base(options) 
        {
        
        }

    }
}
