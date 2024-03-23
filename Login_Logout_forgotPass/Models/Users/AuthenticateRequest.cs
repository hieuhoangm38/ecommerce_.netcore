namespace WebApi.Login_Logout_forgotPass.Models.Users;

using System.ComponentModel.DataAnnotations;

public class AuthenticateRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}