using System.Collections.Generic;

namespace Verademo.Models
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