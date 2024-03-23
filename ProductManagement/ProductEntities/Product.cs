using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.ProductManagement.ProductEntities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [Column("Id")]
        public int ID { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Price")]
        public float Price { get; set; }
        [Column("Description")]
        public string Description { get; set; }
        [Column("Category_id")]
        public int Category_id { get; set; }
        [Column("AddressProduct")]
        public string AddressProduct { get; set; }
    }

}
