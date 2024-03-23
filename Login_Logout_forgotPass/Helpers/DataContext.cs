namespace WebApi.Login_Logout_forgotPass.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Login_Logout_forgotPass.Entities;
using WebApi.ProductManagement.ProductEntities;
using WebApi.ProductManagement.ProductModel;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sql server database
        options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));

    }

    public DbSet<User> Users { get; set; }

    public DbSet<Identify> Identifys { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductCategorie> ProductCategories { get; set; }

    public DbSet<ProductImage> ProductImages { get; set; }


    public DbSet<Image> Images { get; set; }

    //public DbSet<Otp> Otp { get; set; }
}