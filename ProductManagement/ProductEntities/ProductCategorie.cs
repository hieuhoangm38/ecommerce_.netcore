using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.ProductManagement.ProductEntities
{
    [Table("Categorie")]
    public class ProductCategorie
    {

        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Description")]
        public string Description { get; set; }


    }

    //TOP(1000) [Id]
    //  ,[Name]
    //  ,[Description]
    //FROM[Login].[dbo].[Categories]
}
