using System.Collections.Generic;

namespace VeraDemoNet.Models
{
    public class BlabViewModel
    {
        public BlabViewModel()
        {
            Comments = new List<Comment>();
        }
        public string BlabName { get; set; }

        public string Content { get; set; }
        public string Error { get; set; }
        public int BlabId { get; set; }
        public List<Comment> Comments { get; set; }
    }
}