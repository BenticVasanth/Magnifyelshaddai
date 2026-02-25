using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class RegistrationViewModel
    {
        public int RMID { get; set; }
        public int? BWSID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid Email Address")]
        public string EmailId { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Age is required")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Mobile no is required")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "Qualification is required")]
        public int Qualification { get; set; }
        [Required(ErrorMessage = "Participant Type is required")]
        public string ParticipantType { get; set; }
        [Required(ErrorMessage = "Need Of Accommodation is required")]
        public bool NeedOfAccommodation { get; set; }
    }

    public enum AccommodType
    {
        Hall,
        Room
    }

    public enum ParticipaType
    {
        Student,
        Non_Student
    }
}