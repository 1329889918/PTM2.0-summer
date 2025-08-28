using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace PTM2._0.Models
{
    public class Performance
    {
        [Key]
        [Display(Name = "演出编号")]
        public int PerformID { get; set; }

        [Required]
        [Display(Name = "演出名称")]
        [StringLength(200, ErrorMessage = "演出名称长度不能超过200字符")]
        public string PerformName { get; set; }

        [Required]
        [Display(Name = "开始时间")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "结束时间")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Display(Name = "演出日期")]
        public DateTime PerformDate { get; set; }

        [Required]
        [Display(Name = "场地")]
        public int VenueID { get; set; }

        [Required]
        [Display(Name = "演出类型")]
        public PerformanceTypeEnum PerformType { get; set; }

        [Required]
        public PerformanceStatusEnum Status { get; set; }

        [ForeignKey("VenueID")]
        public virtual Venue Venue { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
