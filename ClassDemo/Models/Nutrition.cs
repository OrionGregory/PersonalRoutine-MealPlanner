﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Nutrition
    {
        public int? Id { get; set; }
        public int BMR { get; set; }
        public int CalorieSurplusOrDeficit { get; set; }
        public int RoutineCaloriesBurned { get; set; } // New property
        public int TotalDailyCalories { get; set; } // New property
        public int ProteinPercentage { get; set; }
        public int CarbPercentage { get; set; }
        public int FatPercentage { get; set; }

        // Foreign key to Person
        public int? PersonId { get; set; }
        public Person? Person { get; set; }
    }

}
