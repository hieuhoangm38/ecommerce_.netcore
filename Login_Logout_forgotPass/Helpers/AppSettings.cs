namespace WebApi.Login_Logout_forgotPass.Helpers;

public class AppSettings
{
    public string Secret { get; set; }

    public string EmailHost { get; set; }
    public string EmailUserName { get; set; }
    public string EmailPassword { get; set; }
}