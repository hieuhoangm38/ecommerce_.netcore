using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApi.ProductManagement.ProductEntities;

namespace WebApi.ProductManagement.ProductEntities
{
    [Table("ProductImage")]
    public class ProductImage
    {
        [Key]
        [Column("Id")] 
        
        public int Id { get; set; }

        [Column("ImageData")]
        public byte[] ImageData { get; set; }

        [Column("ImageMimeType")] 
        public string ImageMimeType { get; set; }
        [Column("ProductId")]
        public int ProductId { get; set; }
    }
}
