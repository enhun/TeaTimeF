using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTimeDemo.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "優惠碼")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "描述")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "折扣類型")]
        public string DiscountType { get; set; } // 百分比或固定金額

        [Required]
        [Range(1, 10000)]
        [Display(Name = "折扣值")]
        public double DiscountAmount { get; set; }

        [Required]
        [Display(Name = "最低消費金額")]
        public double MinimumAmount { get; set; }

        [Display(Name = "開始日期")]
        public DateTime StartDate { get; set; }

        [Display(Name = "結束日期")]
        public DateTime EndDate { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "使用次數上限")]
        public int? MaxUsage { get; set; }

        [Display(Name = "已使用次數")]
        public int UsedCount { get; set; } = 0;
    }
}
