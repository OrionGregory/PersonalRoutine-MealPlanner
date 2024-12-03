// Models/ActorTweet.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassDemo.Models
{
    public class ActorTweet
    {
        public int Id { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        public string Tweet { get; set; }

        [Required]
        public string Sentiment { get; set; }

        // Navigation Property
        [ForeignKey("ActorId")]
        public Actor Actor { get; set; }
    }
}
