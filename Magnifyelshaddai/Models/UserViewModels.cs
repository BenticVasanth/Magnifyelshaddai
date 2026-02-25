using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Models
{
    public class UserViewModels
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string Mobile { get; set; }
        //[RegularExpression("^[^@\s]+@[^@\s]+(\.[^@\s]+)+$", ErrorMessage = "Invalid Email Address")]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        //[Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        //[NotMapped]
        //[Required(ErrorMessage = "Confirm Password required")]
        //[System.ComponentModel.DataAnnotations.CompareAttribute("Password", ErrorMessage = "Password doesn't match.")]
        public string ComfirmPassword { get; set; }
        public string UserIP { get; set; }
        public string Location { get; set; }
        public string UserType { get; set; }
        public DateTime? CreatedDateTime { get; set; }

        public bool? IsActive { get; set; }
        public bool IsNotification { get; set; }

        public virtual List<UserViewModels> UserViewModels1 { get; set; }
        public UserViewModels()
        {
            this.UserViewModels1 = new List<UserViewModels>();
        }

        public UserViewModels(User use)
        {
            UserId = use.UserId;
            Name = use.Name;
            Mobile = use.Mobile;
            Email = use.Email;
            Password = use.Password;
            UserIP = use.UserIP;
            Location = use.Location;
            UserType = use.UserType;
            CreatedDateTime = use.CreatedDateTime;
            IsActive = use.IsActive;
            //IsNotification = use.IsNotification;

        }

        public UserViewModels ConvertFromRole(User use)
        {
            UserId = use.UserId;
            Name = use.Name;
            Mobile = use.Mobile;
            Email = use.Email;
            Password = use.Password;
            UserIP = use.UserIP;
            Location = use.Location;
            UserType = use.UserType;
            CreatedDateTime = use.CreatedDateTime;
            IsActive = use.IsActive;
            //IsNotification = use.IsNotification;
            return this;
        }
    }

    public class LoginViewModels
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }
    }
    public class PasswordModel
    {
        [Required(ErrorMessage = "Old Password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password required")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("NewPassword", ErrorMessage = "Password doesn't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}