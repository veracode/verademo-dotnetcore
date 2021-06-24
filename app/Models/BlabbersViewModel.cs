using System.Collections.Generic;

namespace Verademo.Models
{
    public class BlabbersViewModel
    {
        public BlabbersViewModel()
        {
            Blabbers = new List<Blabber>();
        }

        public string Error { get; set; }
        public List<Blabber> Blabbers { get; set; }
    }
}