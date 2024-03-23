using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApi.ProductManagement.ProductModel;
using WebApi.ProductManagement.ProductService;

namespace WebApi.ProductManagement.ProductController
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [AllowAnonymous]
        [HttpPost("addProduct")]
        public async Task<IActionResult> UploadImage
        (
            [FromForm] string datajson,
            [FromForm] IFormFile file = null,
            [FromForm] string imageUrl = null
        )
        {
            AddProductRequest addProductRequest = JsonConvert.DeserializeObject<AddProductRequest>(datajson);

            int productId = _productService.AddProduct(addProductRequest);

            if (productId == -1)
                throw new Exception("lỗi");

            try
            {
                var imageId = await _productService.UploadImage(file, imageUrl,productId);
                return Ok($"Image uploaded successfully. Image ID: {imageId}");
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }

        }
        [AllowAnonymous]
        [HttpGet("getimageId")]
        public IActionResult GetImage([FromForm] int productId, [FromForm] int imageId)
        {
            ProductImageResponse productImageResponse = _productService.GetImage(productId, imageId);

            return Ok(productImageResponse);
        }



        //test api đưa cho hải 
        [AllowAnonymous]
        [HttpPost("upload")]
        public IActionResult UploadImage([FromForm] IFormFile file = null)
        {
            try
            {
                // request.ImagePath chứa đường dẫn ảnh
                // Gọi service để lưu đường dẫn vào cơ sở dữ liệu

                
                var imageId = _productService.SaveImage(file.FileName);

                return Ok(new { ImageId = imageId, Message = "Image uploaded successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Error uploading image: {ex.Message}" });
            }
        }
        [AllowAnonymous]
        [HttpGet("getimage/{imageId}")]
        public IActionResult GetImage(int imageId)
        {
            try
            {
                // Gọi service để lấy đường dẫn ảnh
                var imagePath = _productService.GetImagePath(imageId);

                // Trả về đường dẫn ảnh trong response
                return Ok(new { ImagePath = imagePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Error getting image: {ex.Message}" });
            }
        }


        [AllowAnonymous]
        [HttpGet("get")]
        public IActionResult Get()
        {
            return Ok("hải đb");  
        }


    }
}
