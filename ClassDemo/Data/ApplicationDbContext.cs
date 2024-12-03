using ClassDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClassDemo.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // Extend from IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<AIReview> AIReviews { get; set; }
        public DbSet<ActorTweet> ActorTweets { get; set; } // Added DbSet for ActorTweet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity key types for SQL Server compatibility
            modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasColumnType("nvarchar(450)"));
            modelBuilder.Entity<IdentityUser>(entity => entity.Property(m => m.Id).HasColumnType("nvarchar(450)"));
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasColumnType("nvarchar(450)");
                entity.Property(m => m.RoleId).HasColumnType("nvarchar(450)");
            });

            // Configure composite primary key for MovieActor
            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            // Configure relationships for MovieActor
            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

            // Configure relationship for ActorTweet
            modelBuilder.Entity<ActorTweet>()
                .HasOne(at => at.Actor)
                .WithMany(a => a.ActorTweets)
                .HasForeignKey(at => at.ActorId);
        }
    }
}
