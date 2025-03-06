using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;

namespace TeaTimeDemo.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // 移除這個方法，因為有拼寫錯誤 (Updata -> Update)
        // public void UpdataStatus(int id, string status, string? paymentStatus = null)

        public void UpdateStatus(int id, string status, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrdersHeaders.FirstOrDefault(m => m.Id == id); // 修正: OrdersHeaders -> OrderHeaders
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = status;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void Update(OrderHeader obj)
        {
            _db.OrdersHeaders.Update(obj); // 修正: OrdersHeaders -> OrderHeaders
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            throw new NotImplementedException();
        }

        // 移除這個多餘的方法，因為上面已經有完整的 UpdateStatus 實作
        // public void UpdateStatus(int id, string statusInProcess)
        // {
        //     throw new NotImplementedException();
        // }
    }
}