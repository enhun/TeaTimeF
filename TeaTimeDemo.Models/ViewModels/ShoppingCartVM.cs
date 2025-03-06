using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTimeDemo.Models.ViewModels;
using TeaTimeDemo.Models;
namespace TeaTimeDemo.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public double OrderTotal { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
