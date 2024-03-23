using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.ProductManagement.ProductModel
{
    [Table("images")]
    public class Image
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Path")]
        public string Path { get; set; }
    }
}


