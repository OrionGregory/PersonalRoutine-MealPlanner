using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Sex is required.")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        public float Weight { get; set; }

        [Required(ErrorMessage = "Goal is required.")]
        public float GoalWeight { get; set; }

        [Required(ErrorMessage = "Time is required.")]
        public int Time { get; set; }

        // Navigation property to Routine
        public ICollection<Routine>? Routines { get; set; }

        // Associate with IdentityUser
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}
