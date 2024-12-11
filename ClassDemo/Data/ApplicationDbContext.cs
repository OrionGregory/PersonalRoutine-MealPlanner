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
        public DbSet<CompletedExercise> CompletedExercises { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<IdentityUser> AspNetUsers { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity table names if necessary
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");

            // Configure relationships
            modelBuilder.Entity<Routine>()
                .HasOne(r => r.Person)
                .WithMany(p => p.Routines)
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Nutrition>()
                .HasOne(n => n.Person)
                .WithOne(p => p.Nutrition)
                .HasForeignKey<Nutrition>(n => n.PersonId);

            modelBuilder.Entity<Nutrition>()
                .HasMany(n => n.Meals)
                .WithOne(m => m.Nutrition)
                .HasForeignKey(m => m.NutritionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
