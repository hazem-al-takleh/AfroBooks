using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.Models.ViewModels
{
    public class CartProductViewModel
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Range(1,int.MaxValue)]
        public int? Count { get; set; }
    }
}
