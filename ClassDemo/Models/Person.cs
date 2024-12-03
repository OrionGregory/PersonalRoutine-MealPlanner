using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Person
    {
        [Required(ErrorMessage = "Name is required.")]
        public string name { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        public int age { get; set; }

        [Required(ErrorMessage = "Sex is required.")]
        public string sex { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        public float weight {  get; set; }

        [Required(ErrorMessage = "Goal is required.")]
        public float goalWeight {  get; set; }

        [Required(ErrorMessage = "Time is required.")]
        public int time {  get; set; }
    }
}
