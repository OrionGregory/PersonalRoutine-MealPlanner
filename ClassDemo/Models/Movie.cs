using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassDemo.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Url]
        public string? IMDBLink { get; set; }

        public string Genre { get; set; }

        [Range(1888, 2100)]
        public int Year { get; set; }

        public string? Poster { get; set; } // URL or file path to the poster

        public ICollection<MovieActor>? MovieActors { get; set; }


        // Navigation property for AI reviews
        public ICollection<AIReview>? AIReviews { get; set; } = new List<AIReview>();
    }
}
