using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTimeDemo.DataAccess.Data;
using TeaTimeDemo.DataAccess.Repository.IRepository;
using TeaTimeDemo.Models;
using TeaTimeDemo.Utility;

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
                string oldStatus = orderFromDb.OrderStatus;
                orderFromDb.OrderStatus = status;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }

                // 如果狀態有更改，添加追蹤記錄
                if (oldStatus != status)
                {
                    // 根據狀態添加不同的註解
                    string comment = "";
                    switch (status)
                    {
                        case SD.StatusPending:
                            comment = "訂單已建立，等待處理";
                            break;
                        case SD.StatusInProcess:
                            comment = "廚房已接受訂單，開始製作";
                            break;
                        case SD.StatusReady:
                            comment = "製作完成，可以取餐";
                            break;
                        case SD.StatusCompleted:
                            comment = "訂單已完成";
                            break;
                        case SD.StatusCancelled:
                            comment = "訂單已取消";
                            break;
                    }

                    // 假設 updatedBy 是當前用戶的名稱或ID，這裡需要傳遞該值
                    string updatedBy = "系統"; // 這裡應該替換為實際的用戶名稱或ID
                    AddTracking(id, status, comment, updatedBy);
                }
            }
        }


        public void AddTracking(int orderId, string status, string comments, string updatedBy)
        {
            var tracking = new OrderTracking
            {
                OrderHeaderId = orderId,
                Status = status,
                StatusDate = DateTime.Now,
                Comments = comments,
                UpdatedBy = updatedBy
            };

            _db.OrderTrackings.Add(tracking);
        }

        public List<OrderTracking> GetOrderTrackingHistory(int orderId)
        {
            return _db.OrderTrackings.Where(t => t.OrderHeaderId == orderId)
                .OrderByDescending(t => t.StatusDate).ToList();
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