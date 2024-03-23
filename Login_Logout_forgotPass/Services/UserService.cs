namespace WebApi.Login_Logout_forgotPass.Services;

using AutoMapper;
using BCrypt.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using WebApi.Login_Logout_forgotPass.Authorization;
using WebApi.Login_Logout_forgotPass.Entities;
using WebApi.Login_Logout_forgotPass.Helpers;
using WebApi.Login_Logout_forgotPass.Models.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;
//using IDatabase = StackExchange.Redis.IDatabase;

public interface IUserService
{
    string Authenticate(AuthenticateRequest model);
    IEnumerable<User> GetAll();
    User GetById(int id);
    string Register(RegisterRequest model);
    void Update(int id, UpdateRequest model);
    void Delete(int id);
    AuthenticateResponse VerifyOtp(OtpRequest OtpCode, string ipAddress);
    RefreshTokenResponse RefreshToken(string refreshToken, string ipAddress);
    //T GetData<T>(string key);
    //bool SetData<T>(string key, T value);
    //object RemoveData(string key);
    //public bool IsExist(string key);

    void ForgotPassword(ForgotPasswordRequest emailModel, string origin);
    void Logout(int id);

    void ResetPassword(ResetPasswordRequest resetPasswordModel);

    
}

public class UserService : IUserService
{
    private DataContext _context;
    private IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private IOtpUtils _otpUtils;
    //private IDatabase _redisCache;
    //private readonly StackExchange.Redis.IDatabase _redisCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly StackExchange.Redis.IDatabase _redisCache;
    public UserService(
        DataContext context,
        IJwtUtils jwtUtils,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IOtpUtils otpUtils,
        IConnectionMultiplexer connectionMultiplexer
        )
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _otpUtils = otpUtils;
        //var redisdb = ConnectionMultiplexer.Connect("127.0.0.1:6379,allowAdmin = true");
        _connectionMultiplexer = connectionMultiplexer;
        _redisCache = _connectionMultiplexer.GetDatabase();
    }

    public string Authenticate(AuthenticateRequest model)
    {
        User user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

        // validate
        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash))
            throw new AppException("Username or password is incorrect");

        string otp = _otpUtils.GenerateOtp(user);
        SendEmail(user.Email, otp);

        return $"mã otp đã được gửi đến email {user.Email} mời bạn xác thực mã otp";
    }

    private void SendEmail(string emailParagram, string otp)
    {
        // Gửi mã OTP đến email của người dùng sử dụng MailKit để gửi email
        MimeMessage email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("hoangvanhieudhcn@gmail.com"));
        email.To.Add(MailboxAddress.Parse(emailParagram));
        email.Subject = "Xác minh thông tin đăng nhập";
        email.Body = new TextPart("plain")
        {
            Text = $"Mã OTP của bạn là: {otp}"
        };

        using SmtpClient client = new SmtpClient();

        client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        client.Authenticate("hoangvanhieudhcn@gmail.com", "besu jszy bsyb fvso");
        client.Send(email);
        client.Disconnect(true);


    }

    public IEnumerable<User> GetAll()
    {
        return _context.Users;
    }

    public User GetById(int id)
    {
        return getUser(id);
    }

    public string Register(RegisterRequest model)
    {
        // validate
        if (_context.Users.Any(x => x.Username == model.Username))
            throw new AppException("Username '" + model.Username + "' is already taken");

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                // map model to new user object
                User user = _mapper.Map<User>(model);

                // hash password
                user.PasswordHash = BCrypt.HashPassword(model.Password);

                // save user
                _context.Users.Add(user);
                _context.SaveChanges();

                Identify identify = _mapper.Map<Identify>(model);
                identify.UserId = user.Id;
                // hash password


                // save user
                _context.Identifys.Add(identify);
                _context.SaveChanges();

                // Hoàn thành transaction
                transaction.Commit();

                // Trả về kết quả thành công
                return "Lưu thông tin thành công";
            }
            catch (Exception ex)
            {
                // Xảy ra lỗi, rollback transaction
                transaction.Rollback();

                // Xử lý lỗi và trả về thông báo lỗi
                return "Lưu thông tin thất bại: " + ex.Message;
            }
        }
    }

    public void Update(int id, UpdateRequest model)
    {
        var user = getUser(id);

        // validate
        if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
            throw new AppException("Username '" + model.Username + "' is already taken");

        // hash password if it was entered
        if (!string.IsNullOrEmpty(model.Password))
            user.PasswordHash = BCrypt.HashPassword(model.Password);

        // copy model to user and save
        _mapper.Map(model, user);
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = getUser(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    // helper methods

    private User getUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }

    public AuthenticateResponse VerifyOtp(OtpRequest otpRequest, string ipAddress)
    {
        User user = _context.Users.FirstOrDefault(x => x.Email == otpRequest.Email);

        if (user == null)
        {
            throw new Exception("email nay ko ton tai");
        }

        bool Verified = _otpUtils.VerifyOtpss(user, otpRequest.OtpCode);

        if (Verified)
        {
            AuthenticateResponse response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            response.RefreshToken = _jwtUtils.GenerateRefreshToken(user, ipAddress);
            _redisCache.KeyDelete($"otp:user{user.Id}");

            ////////////////////////////////////////
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(response.Token);

            DateTime timeOut = jwtToken.ValidTo;



            TimeSpan timeRemaining = timeOut - DateTime.UtcNow;

            //Lấy giá trị số giây còn lại
            int secondsRemaining = (int)timeRemaining.TotalSeconds;

            string keySet = $"AllowList:user{user.Id}";
            _redisCache.StringSet(keySet, response.Token, TimeSpan.FromSeconds(secondsRemaining));

            ///////////////////////////////////////////////////////////
            return response;
        }

        else
        {
            throw new Exception("mã otp hết hạn hoặc mã bạn nhập vào bị sai");
        }
    }

    public RefreshTokenResponse RefreshToken(string refreshToken, string ipAddress)
    {
        char separator = '.';
        int index = refreshToken.IndexOf(separator);

        int leftPart = int.Parse(refreshToken.Substring(0, index));

        User user = _context.Users.SingleOrDefault(u => u.Id == leftPart);

        bool check = _redisCache.KeyExists($"RefreshToken:user{user.Id}");

        string valueIp = _redisCache.HashGet($"RefreshToken:user{user.Id}", "CreatedByIp");
        if (check && valueIp == ipAddress)
        {

            RefreshTokenResponse response = _mapper.Map<RefreshTokenResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            return response;
        }
        else
        {
            throw new Exception("refresh token này không tồn tại");
        }
    }

    public void Logout(int id)
    {
        _redisCache.KeyDelete($"AllowList:user{id}");
    }

    public void ForgotPassword(ForgotPasswordRequest emailModel, string origin)
    {
        User user = _context.Users.SingleOrDefault(x => x.Email == emailModel.Email);

        if (user == null)
            return;
        string resetToken = generateResetToken(user);

        _redisCache.StringSet($"ResetOTP:user{user.Id}", resetToken, TimeSpan.FromMinutes(5));

        sendPasswordResetEmail(user, origin);
    }

    private string generateResetToken(User user)
    {

        Random random = new Random();
        const string chars = "0123456789";
        string randomString = new string(Enumerable.Repeat(chars, 4)
          .Select(s => s[random.Next(s.Length)]).ToArray());

        return randomString;
    }

    private void sendPasswordResetEmail(User user, string origin)
    {
        string message;
        if (!string.IsNullOrEmpty(origin))
        {
            var resetUrl = $"{origin}/account/reset-password?token={_redisCache.StringGet($"ResetOTP:user{user.Id}")}";
            message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
        }
        else
        {
            message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{_redisCache.StringGet($"ResetOTP:user{user.Id}")}</code></p>";
        }

        Send(
            to: user.Email,
            subject: "Sign-up Verification API - Reset Password",
            html: $@"<h4>Reset Password Email</h4>
                        {message}"
        );

    }

    private void Send(string to, string subject, string html, string from = null)
    {
        // create message
        var email = new MimeMessage();
        //from truyền vào là phần user name trên ethereal
        email.From.Add(MailboxAddress.Parse("hoangvanhieudhcn@gmail.com"));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        // send email
        using var smtp = new SmtpClient();
        smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("hoangvanhieudhcn@gmail.com", "besu jszy bsyb fvso");
        smtp.Send(email);
        smtp.Disconnect(true);

    }

    public void ResetPassword(ResetPasswordRequest resetPasswordModel)
    {
        User user = getUserByResetToken(resetPasswordModel.Token);

        // update password and remove reset token
        user.PasswordHash = BCrypt.HashPassword(resetPasswordModel.Password);

        _context.Users.Update(user);
        _context.SaveChanges();
    }

    private User getUserByResetToken(string token)
    {
        string input = token;
        int firstDotIndex = input.IndexOf('.'); // Tìm vị trí của dấu chấm đầu tiên

        int leftOfFirstDot = int.Parse(input.Substring(0, firstDotIndex)); // Lấy phần bên trái của dấu chấm đầu tiên

        User user = _context.Users.SingleOrDefault(x => x.Id == leftOfFirstDot);

        if (user == null)
        {
            throw new Exception("ko có người dùng này");
        }

        return user;
    }

   
}