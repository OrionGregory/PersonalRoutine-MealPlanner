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
            // Configure Person -> Routines relationship
            modelBuilder.Entity<Person>()
                .HasMany(p => p.Routines) // A Person has many Routines
                .WithOne(r => r.Person)   // Each Routine belongs to one Person
                .HasForeignKey(r => r.PersonId); // Foreign key in Routine pointing to Person

            // Configure Routine -> Exercises relationship
            modelBuilder.Entity<Routine>()
                .HasMany(r => r.Exercises) // A Routine has many Exercises
                .WithOne(e => e.Routine)   // Each Exercise belongs to one Routine
                .HasForeignKey(e => e.RoutineId); // Foreign key in Exercise pointing to Routine

            base.OnModelCreating(modelBuilder);
        }

    }
}
