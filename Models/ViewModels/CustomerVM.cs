using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Khati.Models.ViewModels
{
    public class CustomerVM
    {
        public CustomerVM()
        {
            this.SelectedFishIds = new List<int>();
            this.SelectedVegetableIds = new List<int>();
        }
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(50, ErrorMessage = "Customer Name cannot be longer than 50 characters.")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^[0-9]{10,15}$", ErrorMessage = "Invalid Phone Number. Please enter 10-15 digits.")]
        public int Phone { get; set; }
        [Required, Display(Name = "Last Purchase Date"), DataType(DataType.Date),
         DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfPurchase { get; set; }
        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }
        public string Picture { get; set; }
        [Display(Name = "Picture File")]
        [NotMapped]
        public HttpPostedFileBase PictureFile { get; set; }
        [Display(Name = "Member of Shop")]
        public bool IsMember { get; set; }
        public int TotalPrice { get; set; }
        public List<int> SelectedFishIds { get; set; }
        public List<int> SelectedVegetableIds { get; set; }
        public List<ProductItem> PurchasedItems { get; set; }
    }
}
