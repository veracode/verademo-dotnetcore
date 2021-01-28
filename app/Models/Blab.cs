using System;

namespace Verademo.Models
{
    public class Blab
    {
        public Blabber Author { get; set; }
        public string Content { get; set; }
        public DateTime PostDate { get; set; }
        public string PostDateString => PostDate.ToString("MMM d, yyyy");
        public int CommentCount { get; set; }
        public int Id { get; set; }
        
    }
}