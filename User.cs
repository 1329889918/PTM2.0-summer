using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PTM2._0.Models
{
    public class User
    {
        [Key]
        [Display(Name = "用户ID")]
        public int UserID { get; set; }

        [Required]
        [Display(Name = "用户名")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "出生日期")]
        [Column(TypeName = "date")]
        public DateTime Birthdate { get; set; }

        [Required]
        [Display(Name = "居住地址")]
        [StringLength(255)]
        public string Address { get; set; }

        [Display(Name = "邮箱")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "电话")]
        [StringLength(15, ErrorMessage = "长度不符,必须为11-15之间！", MinimumLength = 11)]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "性别")]
        public Gender Gender { get; set; }

        [Required]
        [Display(Name = "密码")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "密码长度至少为6位")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "是否管理员")]
        public bool IsAdmin { get; set; } = false;
        public virtual ICollection<Order> Orders
        {
            get; set;
        }
    }
}

