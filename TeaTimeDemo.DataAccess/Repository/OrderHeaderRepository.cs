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

        public void DeleteOrder(int id)
        {
            try
            {
                // First delete related OrderDetails
                var orderDetails = _db.OrderDetails.Where(d => d.OrderHeaderId == id).ToList();
                if (orderDetails.Any())
                {
                    _db.OrderDetails.RemoveRange(orderDetails);
                }

                // Then delete OrderHeader
                var orderHeader = _db.OrdersHeaders.FirstOrDefault(h => h.Id == id);
                if (orderHeader != null)
                {
                    _db.OrdersHeaders.Remove(orderHeader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"刪除訂單時發生錯誤: {ex.Message}", ex);
            }
        }

        public void UpdateStatus(int id, string status, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrdersHeaders.FirstOrDefault(m => m.Id == id);
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
            _db.OrdersHeaders.Update(obj);
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrdersHeaders.FirstOrDefault(m => m.Id == id);
            if (orderFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderFromDb.PaymentIntentId = paymentIntentId;
                }
            }
        }
    }
}