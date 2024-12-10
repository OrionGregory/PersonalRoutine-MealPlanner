using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Assignment3.Models
{
    public class Nutrition
    {
        [Key]
        public int Id { get; set; }

        public int BMR { get; set; }

        public int CalorieSurplusOrDeficit { get; set; }

        public int CarbPercentage { get; set; }

        public int FatPercentage { get; set; }

        public int ProteinPercentage { get; set; }

        public int? RoutineCaloriesBurned { get; set; }

        public int? TotalDailyCalories { get; set; }

        // Foreign key to Person
        public int? PersonId { get; set; }

        public Person? Person { get; set; }

        [Required]
        public ICollection<Meal>? Meals { get; set; } = new List<Meal>();
    }

}
