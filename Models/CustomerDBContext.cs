using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Khati.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Phone { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public int Age { get; set; }
        public string Picture { get; set; }
        public bool IsMember { get; set; }
        public int TotalPrice { get; set; }
        public virtual ICollection<ProductItem> ProductItems { get; set; }
    }

    public class Fish
    {
        public Fish()
        {
            this.ProductItems = new List<ProductItem>();
        }
        [Key]
        public int FishId { get; set; }
        public string FishName { get; set; }
        public int UnitPrice { get; set; }
        public string Picture { get; set; }
        [NotMapped]
        public HttpPostedFileBase PictureFile { get; set; }
        public virtual ICollection<ProductItem> ProductItems { get; set; }
    }

    public class Vegetable
    {
        public Vegetable()
        {
            this.ProductItems = new List<ProductItem>();
        }
        [Key]
        public int VegetableId { get; set; }
        public string VegetableName { get; set; }
        public int UnitPrice { get; set; }
        public string Picture { get; set; }
        [NotMapped]
        public HttpPostedFileBase PictureFile { get; set; }
        public virtual ICollection<ProductItem> ProductItems { get; set; }
    }

    public class ProductItem
    {
        public int ProductItemId { get; set; }        
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }       
        [ForeignKey("Fish")]
        public int? FishId { get; set; } 
        public virtual Fish Fish { get; set; }
        [ForeignKey("Vegetable")]
        public int? VegetableId { get; set; }
        public virtual Vegetable Vegetable { get; set; }
    }

}