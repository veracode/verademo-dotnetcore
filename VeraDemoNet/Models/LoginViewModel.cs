using System.ComponentModel.DataAnnotations;

namespace VeraDemoNet.Models
{
    public class LoginView
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string ReturnURL { get; set; }
    }
}