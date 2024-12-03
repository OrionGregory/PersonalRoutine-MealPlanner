using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Models/AIReview.cs
namespace ClassDemo.Models
{
    public class AIReview
    {
        public int Id { get; set; }
        public string Review { get; set; }
        public string Sentiment { get; set; }
        public int MovieId { get; set; } // Foreign key to associate with Movie
        public Movie Movie { get; set; }
    }
}
