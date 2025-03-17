using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTimeDemo.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Employee = "Employee";
        public const string Role_Manager = "Manager";
        public const string Role_Admin = "Admin";


        // 訂單狀態
        public const string StatusPending = "待處理";
        public const string StatusInProcess = "處理中";
        public const string StatusReady = "可取餐";
        public const string StatusCompleted = "已完成";
        public const string StatusCancelled = "已取消";

        // 訂單狀態詳細說明
        public static Dictionary<string, string> StatusDescriptions = new Dictionary<string, string>
        {
            { StatusPending, "訂單已收到，等待確認" },
            { StatusInProcess, "訂單正在製作中" },
            { StatusReady, "訂單已製作完成，可以取餐" },
            { StatusCompleted, "訂單已完成" },
            { StatusCancelled, "訂單已取消" }
        };

    }
}
