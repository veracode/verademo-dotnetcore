using System.ComponentModel;

namespace Verademo.Models
{
    public class RegisterViewModel
    {
        public string Error { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        public string BlabName { get; set; }
        public string RealName { get; set; }
    }
}