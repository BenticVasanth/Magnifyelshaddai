using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class WebGridTestController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index(int page = 1, string sort = "FirstName", string sortdir = "asc", string search = "")
        {
            int pageSize = 10;
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;
            var data = GetSabbathDayDocuments(search, sort, sortdir, skip, pageSize, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;
            return View(data);
        }

        public DocumentView GetSabbathDayDocuments(string search, string sort, string sortdir, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstSabbathDayDocuments = db.Documents.Where(s => s.Title.Contains(search)).Where(s => s.Status == true).OrderByDescending(s => s.IntId).ToList();
                totalRecord = document.lstSabbathDayDocuments.Count();
               
                if (pageSize > 0)
                {
                    document.lstSabbathDayDocuments = document.lstSabbathDayDocuments.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }
    }
}
