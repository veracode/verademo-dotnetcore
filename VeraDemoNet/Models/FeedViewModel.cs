using System.Collections.Generic;

namespace VeraDemoNet.Models
{
    public class FeedViewModel
    {
        public FeedViewModel()
        {
            BlabsByOthers = new List<Blab>();
            BlabsByMe = new List<Blab>();
        }
        public string CurrentUser { get; set; }
        public string Error { get; set; }
        public List<Blab> BlabsByOthers { get; set; }
        public List<Blab> BlabsByMe { get; set; }
    }
}