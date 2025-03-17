using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeaTimeDemo.Models
{
    public class OrderTracking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderHeaderId { get; set; }

        [ForeignKey("OrderHeaderId")]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime StatusDate { get; set; }

        public string Comments { get; set; }

        // 可選：記錄由誰更新的狀態
        public string UpdatedBy { get; set; }
    }
}

