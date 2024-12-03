using System.Collections.Generic;

namespace Assignment3.Models
{
    public class Routine
    {
        public int Id { get; set; }

        // Foreign key to Person
        public int PersonId { get; set; }
        public Person Person { get; set; }

        // Collection of Exercises
        public List<Exercise> Exercises { get; set; }
    }
}
