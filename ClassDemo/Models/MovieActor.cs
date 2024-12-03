// Models/MovieActor.cs
using System.ComponentModel.DataAnnotations;

namespace ClassDemo.Models
{
    public class MovieActor
    {
        [Key]
        //public int? Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a valid Movie.")]
        public int? MovieId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a valid Actor.")]
        public int? ActorId { get; set; }

        public Movie? Movie { get; set; }
        public Actor? Actor { get; set; }
    }
}
