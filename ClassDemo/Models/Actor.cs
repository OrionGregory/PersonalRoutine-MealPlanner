// Models/Actor.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClassDemo.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Range(0, 150, ErrorMessage = "Age must be between 0 and 150.")]
        public int Age { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? IMDBLink { get; set; }

        public byte[]? Photo { get; set; }

        // Navigation Properties
        public ICollection<MovieActor>? MovieActors { get; set; }
        public ICollection<ActorTweet>? ActorTweets { get; set; }
    }
}
