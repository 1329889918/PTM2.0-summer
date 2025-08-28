using System;
using System.ComponentModel.DataAnnotations;
using PTM2._0.Models;
using PTM2._0.ViewModels;


namespace PTM2._0.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        [Display(Name = "用户名")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
        public string Username { get; set; }

        [Required(ErrorMessage = "请输入出生日期")]
        [Display(Name = "出生日期")]
        public DateTime Birthdate { get; set; }

        [Required(ErrorMessage = "请输入居住地址")]
        [Display(Name = "居住地址")]
        [StringLength(255)]
        public string Address { get; set; }

        [Display(Name = "邮箱")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        public string Email { get; set; }

        [Display(Name = "电话")]
        [StringLength(15, ErrorMessage = "长度不符,必须为11-15之间！", MinimumLength = 11)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "请选择性别")]
        [Display(Name = "性别")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "密码")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "密码长度至少为6位")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "请确认密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }
    }
}