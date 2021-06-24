using System.ComponentModel.DataAnnotations;

namespace Verademo.Models
{
    public class LoginView
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string ReturnURL { get; set; }
        public bool RememberLogin { get; set; }
    }
}