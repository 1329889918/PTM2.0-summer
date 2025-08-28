using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PTM2._0.Models
{
    public class Order
    {
        [Key]
        [Display(Name = "订单编号")]
        public int OrderID { get; set; }
        [Required(ErrorMessage = "用户ID是必填项")]
        [Display(Name = "用户ID")]
        public int UserID { get; set; }
        [Required(ErrorMessage = "购买时间是必填项")]
        [Display(Name = "购买时间")]
        public DateTime OrderTime { get; set; }
        [Required(ErrorMessage = "门票编号是必填项")]
        [Display(Name = "门票名称")]
        public int TicketID { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "每次最多购买5张门票")]
        [Display(Name = "购买门票数量")]
        public int OrderQuantity { get; set; }
        [Required]
        [Display(Name = "总购买金额")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.待支付;

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("TicketID")]
        public virtual Ticket Ticket { get; set; }
        [NotMapped]
        [Display(Name = "演出名称")]
        public string PerformName => Ticket?.Performance?.PerformName ?? "未知演出";
    }
}
