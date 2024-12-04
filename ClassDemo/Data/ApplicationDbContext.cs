using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Assignment3.Models;
using Microsoft.AspNetCore.Identity;

namespace Assignment3.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Routine> Routines { get; set; }
        public DbSet<Exercise> Exercises { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if not using conventions
            modelBuilder.Entity<Routine>()
                .HasOne(r => r.Person)
                .WithMany(p => p.Routines)
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Routine)
                .WithMany(r => r.Exercises)
                .HasForeignKey(e => e.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
