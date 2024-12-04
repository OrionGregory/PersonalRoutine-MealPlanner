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
        public DbSet<Nutrition> Nutrition { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if not using conventions
            modelBuilder.Entity<Routine>()
                .HasOne(r => r.Person)
                .WithMany(p => p.Routines)
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Person -> Nutrition relationship
            modelBuilder.Entity<Nutrition>()
                .HasOne(n => n.Person)   // Each Nutrition belongs to one Person
                .WithOne(p => p.Nutrition) // A Person can have one Nutrition
                .HasForeignKey<Nutrition>(n => n.Id); // Foreign key in Nutrition pointing to Person

            base.OnModelCreating(modelBuilder);
        }
    }
}
