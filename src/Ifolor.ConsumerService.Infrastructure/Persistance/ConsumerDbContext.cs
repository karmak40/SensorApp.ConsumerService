using Ifolor.ConsumerService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ifolor.ConsumerService.Infrastructure.Persistance
{
    public class ConsumerDbContext : DbContext
    {
        public DbSet<SensorEventEntity> SensorEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mydatabase.db;");
        }

        public ConsumerDbContext(DbContextOptions<ConsumerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorEventEntity>(entity =>
            {
                entity.HasKey(e => e.Id); 

                entity.Property(e => e.Status)
                      .HasConversion<string>();
            });

        }
    }
}
