using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        // Foreign key to Nutrition
        public int NutritionId { get; set; }

        [Required]
        [ForeignKey("NutritionId")]
        public Nutrition Nutrition { get; set; }
    }
}