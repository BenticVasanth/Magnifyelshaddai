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
        public string MobileNo { get; set; }
        public string Place { get; set; }
        public bool? AccommodationStatus { get; set; }
        public bool AccomStatus { get; set; }
        public string AccommodationType { get; set; }
        public string ParticipationType { get; set; }
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