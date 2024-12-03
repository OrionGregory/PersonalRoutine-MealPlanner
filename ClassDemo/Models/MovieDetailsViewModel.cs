// Models/ViewModels/MovieDetailsViewModel.cs
using System.Collections.Generic;

namespace ClassDemo.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public string Title { get; set; } // Page title

        public Movie Movie { get; set; }
        public List<Actor> Actors { get; set; }
        public List<AIReview> AIReviews { get; set; }
        public string OverallSentiment { get; set; }
        public string AIError { get; set; }
    }
}
