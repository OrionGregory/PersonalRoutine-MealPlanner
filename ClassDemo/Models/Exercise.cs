using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseId { get; set; }

        [Required(ErrorMessage = "Exercise name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Muscle group is required.")]
        public string MuscleGroup { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        public int Sets { get; set; }

        public int Repetitions { get; set; }
    }

}
