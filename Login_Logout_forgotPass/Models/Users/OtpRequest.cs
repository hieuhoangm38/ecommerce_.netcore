namespace WebApi.Login_Logout_forgotPass.Models.Users
{
    public class OtpRequest
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }
}
