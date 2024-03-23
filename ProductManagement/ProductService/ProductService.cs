using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Login_Logout_forgotPass.Entities;
using WebApi.Login_Logout_forgotPass.Helpers;
using WebApi.Login_Logout_forgotPass.Models.Users;
using WebApi.ProductManagement.ProductEntities;
using WebApi.ProductManagement.ProductModel;
using static System.Net.Mime.MediaTypeNames;
using Image = WebApi.ProductManagement.ProductModel.Image;

namespace WebApi.ProductManagement.ProductService
{
    
    public interface IProductService
    {
        int AddProduct(AddProductRequest addProductRequest);

        Task<int> UploadImage(IFormFile file, string imageUrl, int productId);
        ProductImageResponse GetImage(int productId,int imageId);

        //test api làm service
        int SaveImage(string imagePath);

        string GetImagePath(int imageId);
    }
    public class ProductService : IProductService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        public ProductService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<int> UploadImage(IFormFile file , string imageUrl,int productId)
        {
            if (file == null && string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException("cần truyền vào thông tin file vì nó đang rỗng");
            }

            var image = new ProductImage();

            // Process image from the file uploaded
            if (file != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("file không nằm trong các phần mở rộng cho phép " +
                        ".jpg, .jpeg, .png, .gif.");
                }

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    image.ImageData = ms.ToArray();
                    image.ImageMimeType = file.ContentType;
                }
            }

            // Process image from the URL
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    using (var client = new HttpClient())
                    using (var stream = await client.GetStreamAsync(imageUrl))
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        image.ImageData = ms.ToArray();
                    }

                     
                    image.ImageMimeType = "image/jpeg";
                }
                catch (Exception ex)
                {
                    
                    throw new Exception($"url này bị lỗi: {ex.Message}");
                }
            }
            image.ProductId = productId;
            _dataContext.ProductImages.Add(image);
            await _dataContext.SaveChangesAsync();

            return image.Id;
        }

        public ProductImageResponse GetImage(int productId,int imageId)
        {
            _dataContext.Products.Where(x => x.ID == productId);
            if (_dataContext.Products.Where(x => x.ID == productId) == null) 
                throw new KeyNotFoundException("User not found");

            ProductImage image = _dataContext.ProductImages.Where(x => x.ProductId == productId).
                                                                    FirstOrDefault(x => x.Id ==imageId);
            if (image == null)
            {
                throw new KeyNotFoundException("User not found"); 
            }

            var imageDataUrl = $"data:{image.ImageMimeType};base64,{Convert.ToBase64String(image.ImageData)}";

            return new ProductImageResponse { ImageUrl = imageDataUrl };
        }

        public int AddProduct(AddProductRequest addProductRequest)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    // map model to new user object
                    Product product = _mapper.Map<Product>(addProductRequest);

                    // save user
                    _dataContext.Products.Add(product);
                    _dataContext.SaveChanges();

                    transaction.Commit();

                    return product.ID;
                }
                catch
                {
                   
                    transaction.Rollback();

                }
            }

            return -1;
        }

        //đoạn này để test api gọi
        public int SaveImage(string imagePath)
        {
            // Lưu đường dẫn vào cơ sở dữ liệu
            var imageEntity = new Image
            {
                Path = imagePath,
                // Các thuộc tính khác nếu cần thiết
            };

            _dataContext.Images.Add(imageEntity);
            _dataContext.SaveChanges();

            return imageEntity.Id;
        }

        public string GetImagePath(int imageId)
        {
            var imageEntity = _dataContext.Images.Find(imageId);
            if (imageEntity == null)
            {
                throw new InvalidOperationException($"Image with ID {imageId} not found.");
            }

            return imageEntity.Path;
        }
    }

}

