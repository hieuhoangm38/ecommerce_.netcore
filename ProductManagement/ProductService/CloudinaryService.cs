using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

namespace WebApi.ProductManagement.ProductService
{

public interface ICloudinaryService
    {
        
    }

public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;


        public CloudinaryService()
        {
            Account account = new Account(
                "dmqldz89x",
                "464295252697494",
                "s26AT8KoGEnEY7lbO1a0JQuYgJo");

            _cloudinary = new Cloudinary(account);
        }

        public string UploadImage(IFormFile file, string folderName)
        {
            using (var stream = file.OpenReadStream())
            {

                var uploadParams = new ImageUploadParams
                {
                    Folder = folderName,
                    File = new FileDescription(file.FileName, stream)
                };

                var uploadResult = _cloudinary.Upload(uploadParams);

                if (uploadResult.Error != null && uploadResult.Error.Message.Contains("already exists"))
                {
                    return "0";
                }
                var str = uploadResult.SecureUrl.ToString();

                if (str.Substring(str.Length - 4).ToUpper().Equals("AVIF"))
                {
                    return str.Substring(0, str.Length - 4) + "png";
                }

                return str;

            }
        }
    }

}
