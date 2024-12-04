using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class Nutrition
    {
        public int Id { get; set; }
        public int PersonId { get; set; } // Foreign Key
        public Person Person { get; set; } // Navigation Property


        [Range(0, 5000)]
        public int BMR { get; set; }

        [Range(-2000, 2000)]
        public int CalorieSurplusOrDeficit { get; set; }

        [Range(0, 100)]
        public int ProteinPercentage { get; set; }

        [Range(0, 100)]
        public int CarbPercentage { get; set; }

        [Range(0, 100)]
        public int FatPercentage { get; set; }
    }

}
