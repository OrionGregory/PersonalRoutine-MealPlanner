using ClassDemo.Models;
// Models/ActorDetailsViewModel.cs
using System.Collections.Generic;

namespace ClassDemo.Models
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<Movie> Movies { get; set; }
        public List<ActorTweet> TweetSentiments { get; set; }
        public string OverallSentiment { get; set; }
    }
}
