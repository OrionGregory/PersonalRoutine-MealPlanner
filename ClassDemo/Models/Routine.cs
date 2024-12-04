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

        // Routine type for categorization (e.g., Push, Pull, Legs)
        public string RoutineType { get; set; }

        // Day of the week for this routine
        public string DayOfWeek { get; set; }
    }
}
