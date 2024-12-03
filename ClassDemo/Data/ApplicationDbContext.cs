using Assignment3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

            // Configure one-to-one relationship between Person and Routine
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Routine)
                .WithOne(r => r.Person)
                .HasForeignKey<Routine>(r => r.PersonId);

            // Configure one-to-many relationship between Routine and Exercises
            modelBuilder.Entity<Routine>()
                .HasMany(r => r.Exercises)
                .WithOne(e => e.Routine)
                .HasForeignKey(e => e.RoutineId);
        }
    }
}
