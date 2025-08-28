using System.ComponentModel.DataAnnotations;

namespace PTM2._0.Models
{
    public class LoginModel
    {
        [Key]
        [Display(Name = "用户名")]
        public string Name { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }
    }
}
