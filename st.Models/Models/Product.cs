using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stModels.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display (Name ="Book Title")]
        [MaxLength(100)]
        public string? Title { get; set; }
        [Required]
        public string? ISBN { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Author { get; set; }
        public string? Description { get; set; }
        [Required]
        [Display(Name = "List Price")]
        [Range(1,100)]
        public double ListPrice { get; set; }
        [Range(1, 100)]
        public double Price { get; set; }
        [Range(1, 100)]
        public double Price50 { get; set; }
        [Range(1, 100)]
        public double Price100 { get; set; }
        [ValidateNever]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }
        [ValidateNever]
        public string? imageUrl { get; set; }
    }
}
