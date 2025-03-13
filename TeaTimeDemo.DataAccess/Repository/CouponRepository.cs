// TeaTimeDemo.DataAccess/Repository/CouponRepository.cs
using System;
using System.Linq;
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class CouponRepository : Repository<Coupon>, ICouponRepository
    {
        private readonly ApplicationDbContext _db;

        public CouponRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Coupon coupon)
        {
            _db.Coupons.Update(coupon);
        }

        public bool ValidateCoupon(string code, double orderTotal)
        {
            // 使用不區分大小寫的比較
            var coupon = _db.Coupons.FirstOrDefault(c => c.Code.ToLower() == code.ToLower() && c.IsActive);
            if (coupon == null)
                return false;

            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
                return false;

            if (orderTotal < coupon.MinimumAmount)
                return false;

            if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
                return false;

            return true;
        }


        public double GetDiscountAmount(string code, double orderTotal)
        {
            if (!ValidateCoupon(code, orderTotal))
                return 0;

            var coupon = _db.Coupons.FirstOrDefault(c => c.Code == code);
            if (coupon == null)
                return 0;

            if (coupon.DiscountType == "Percentage")
            {
                return (orderTotal * coupon.DiscountAmount) / 100;
            }
            else // Fixed Amount
            {
                return coupon.DiscountAmount;
            }
        }

        public void IncrementCouponUsage(string code)
        {
            var coupon = _db.Coupons.FirstOrDefault(c => c.Code == code);
            if (coupon != null)
            {
                coupon.UsedCount++;
                Update(coupon);
            }
        }
    }
}
