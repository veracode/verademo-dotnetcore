using System;

namespace Verademo.Models
{
    public class Blabber
    {
        public string UserName { get; set; }
        public string BlabName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreateDateString => CreatedDate.ToString("MMM d, yyyy");
        public int NumberListeners { get; set; }
        public int NumberListening { get; set; }
        public bool Subscribed { get; set; }
    }
}