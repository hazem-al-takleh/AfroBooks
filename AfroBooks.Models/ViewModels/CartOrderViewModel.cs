using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.Models.ViewModels
{
	public class CartOrderViewModel
	{
        public IEnumerable<CartProduct> CartProducts { get; set; }
        // contained in orderHeader
        //public double OrderTotal { get; set; }
        public OrderHeader OrderHeader { get; set; }


    }
}
