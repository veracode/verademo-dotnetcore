using System.Collections.Generic;

namespace Verademo.Models
{
    public class ProfileViewModel
    {
        public string Error { get; set; }
        public List<string> Events { get; set; }
        public List<Blabber> Hecklers { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string Image { get; set; }
        public string BlabName { get; set; }

        public bool IsAdmin { get; set; }
    }
}