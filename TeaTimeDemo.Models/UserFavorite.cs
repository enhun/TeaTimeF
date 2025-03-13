using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TeaTimeDemo.Models/UserFavorite.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TeaTimeDemo.Models
{
    public class UserFavorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
