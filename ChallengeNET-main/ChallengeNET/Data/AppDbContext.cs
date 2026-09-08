using ClyvoCare.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClyvoCare.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<LogSaude> LogsSaude { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pet>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LogSaude>()
                .HasOne<Pet>()
                .WithMany()
                .HasForeignKey(l => l.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
