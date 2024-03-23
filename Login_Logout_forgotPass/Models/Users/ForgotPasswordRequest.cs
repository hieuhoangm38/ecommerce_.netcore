using System.ComponentModel.DataAnnotations;

namespace WebApi.Login_Logout_forgotPass.Models.Users
{
    public class ForgotPasswordRequest
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
