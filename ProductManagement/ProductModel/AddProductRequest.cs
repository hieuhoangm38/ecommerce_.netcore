using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApi.ProductManagement.ProductModel
{
    public class AddProductRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public float Price { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string AddressProduct { get; set; }
        [Required]
        public int Category_id { get; set; }

    }
}
   