using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        [MaxLength(10000)]
        public string Description { get; set; }
        [Required]
        [MinLength(13)]
        [MaxLength(13)]
        [DisplayName("ISBN-13")]
        public string ISBN { get; set; }
        [Required]
        [MaxLength(250)]
        public string Author { get; set; }
        [Required]
        [DisplayName("List Price")]
        public double ListPrice { get; set; }
        [Required]
        [DisplayName("Price for 1-50")]
        public double PriceUnit { get; set; }
        [Required]
        [DisplayName("Price for 51-100")]
        public double Price50Unit { get; set; }
        [Required]
        [DisplayName("Price for 100+")]
        public double Price100Unit { get; set; }
        [ValidateNever]
        public string CoverImageURL { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category ProductCategory { get; set; }

        [Required]
        public int CoverTypeId { get; set; }
        [ForeignKey("CoverTypeId")]
        [ValidateNever]
        public CoverType ProductCoverType { get; set; }
    }
}
