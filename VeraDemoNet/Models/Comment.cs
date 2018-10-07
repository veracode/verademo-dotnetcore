using System;

namespace VeraDemoNet.Models
{
    public class Comment
    {
        public Blabber Author { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TimestampString => TimeStamp.ToString("MMM d, yyyy");
    }
}