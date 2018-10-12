using System.Collections.Generic;

namespace VeraDemoNet.Models
{
    public class SearchBlabsViewModel
    {
        public SearchBlabsViewModel()
        {
            Blabs = new List<BlabSearchResultViewModel>();
        }

        public string Error { get; set; }

        public string SearchText { get; set; }

        public List<BlabSearchResultViewModel> Blabs { get; set; }
    }
}