using System;

namespace Verademo.Models
{
    public class BlabSearchResultViewModel
    {
        public string Content { get; set; }
        public string Blabber { get; set; }
        public DateTime BlabDate { get; set; }
        public string BlabDateString => BlabDate.ToString("MMM d, yyyy");
    }
}