using Microsoft.EntityFrameworkCore;

namespace WorldCollector.Yande.Models
{
    public class YandeDbContext : DbContext
    {
        public DbSet<CollectRecord> CollectRecords { get; set; }

        public YandeDbContext()
        {
        }

        public YandeDbContext(DbContextOptions<YandeDbContext> options) : base(options)
        {
        }


    }
}
