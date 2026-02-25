using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Models
{
    public class DocumentViewModels
    {
        public int IntId { get; set; }
        [Required(ErrorMessage = "The Document ID field is required")]
        public string DocumentID { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        [Required(ErrorMessage = "The Category name field is required")]
        public int? CategoryId { get; set; }
        [Required(ErrorMessage = "The Title field is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "The Description field is required")]
        public string Description { get; set; }
        public DateTime PrayerDate { get; set; }
        [Required(ErrorMessage = "The File Path field is required")]
        public string FilePath { get; set; }
        public int? DownloadCount { get; set; }
        public string DocumentReference { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public bool? Status { get; set; }
        public string ImagesPath { get; set; }

        public string EMailBody { get; set; }
        [Display(Name = "Email Subject")]
        public string EmailSubject { get; set; }

        [Required]
        public HttpPostedFileBase DocUpload { get; set; }
        public HttpPostedFileBase ImageUpload { get; set; }
    }
}