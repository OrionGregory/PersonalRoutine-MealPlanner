using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Routine
    {
        [Key]
        public int RoutineId { get; set; }

        [Required(ErrorMessage = "Routine name is required.")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Exercises are required.")]
        public List<Exercise> Exercises { get; set; }

        [Required(ErrorMessage = "Frequency is required.")]
        public int FrequencyPerWeek { get; set; }

    }
}
