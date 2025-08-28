using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PTM2._0.Models
{
    public class Ticket
    {
        [Key]
        [Display(Name = "门票编号")]
        public int TicketID { get; set; }

        [Required]
        [Range(1, 2000, ErrorMessage = "价格不能超过2000")]
        [Display(Name = "价格")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "演出名称")]
        public int PerformID { get; set; }

        [Required]
        [Display(Name = "剩余门票数量")]

        public int TicketQuantity { get; set; }

        [Required]
        [Display(Name = "初始总票数")]
        [Column(TypeName = "int")]
        public int InitialTicketQuantity { get; set; }


        [ForeignKey("PerformID")]
        public virtual Performance Performance { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        [Display(Name = "售票百分比")]
        public double SoldPercentage { get; set; }
    }
}
