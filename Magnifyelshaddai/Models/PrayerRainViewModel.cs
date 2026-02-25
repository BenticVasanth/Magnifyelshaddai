using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Magnifyelshaddai.Models
{
    public class PrayerRainViewModel
    {
        public int PrayerRainId { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Verses is required")]
        public string Verses { get; set; }
        [Required(ErrorMessage = "Prayer is required")]
        public string Prayer { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDateAndTime { get; set; }
        public Nullable<System.DateTime> EditEndDateAndTime { get; set; }
        public string PrayerRainFilePath { get; set; }
        public Nullable<bool> IsEditable { get; set; }
        public Nullable<bool> IsEmailSent { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDateTime { get; set; }
        public string RefPrayerRainId { get; set; }
        public string FileName { get; set; }
    }
}