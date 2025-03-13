using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository.IRepository
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        void Update(Coupon coupon);
        bool ValidateCoupon(string code, double orderTotal);
        double GetDiscountAmount(string code, double orderTotal);
        void IncrementCouponUsage(string code);
    }
}
