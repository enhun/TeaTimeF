using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeaTimeDemo.Models
{
    public class Story
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }  // 修改這裡，添加 ? 使其可為空

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // 關聯到產品 (可為空，表示不一定要關聯到特定產品)
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // 故事類型：0=老奶奶的故事，1=產品故事，2=部落格文章
        public int StoryType { get; set; }

        // 是否置頂
        public bool IsFeatured { get; set; }
    }
}