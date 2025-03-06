using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTimeDemo.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ShoppingCart Product { get; set; }
        public IEnumerable<Review> Reviews { get; set; }

        // 新增評論相關屬性
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}