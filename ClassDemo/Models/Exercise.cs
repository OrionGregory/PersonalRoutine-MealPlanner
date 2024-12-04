using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int Reps { get; set; }

        [Required]
        public int Sets { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        // Foreign key to WorkoutPlan
        public int? RoutineId { get; set; }
        public Routine? Routine { get; set; }
    }
}
