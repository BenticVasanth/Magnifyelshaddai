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
    public class EBookController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult EBook(int page = 1, string search = "")
        {
            List<EBookVM> lstEBookDownloadDetail = new List<EBookVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetEBooks(search, skip, pageSize, out totalRecord);
                foreach (var eBook in data.lstEBooks)
                {
                    List<string> lstEBookDownloadedUsers = db.EBooksDownloadDetails.Where(eb => eb.BooKId == eBook.BooKId).Select(eb => eb.DownloadedBy).Distinct().ToList();
                    List<EBooksDownloadDetail> lstEBookDownloadDetails = new List<EBooksDownloadDetail>();
                    foreach (string user in lstEBookDownloadedUsers)
                    {
                        lstEBookDownloadDetails.Add(new EBooksDownloadDetail { DownloadedBy = user });
                    }
                    eBook.totalDownloadedCount = lstEBookDownloadedUsers.Count();
                    lstEBookDownloadDetail.Add(new EBookVM { eBook = eBook, lstEBookDownloadDetails = lstEBookDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstEBookDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetEBooks(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstEBooks = db.EBooksIndexes.Where(e => e.Title.Contains(search) || e.Author.Contains(search)).Where(e => e.IsActive == true).OrderByDescending(e => e.BooKId).ToList();
                totalRecord = document.lstEBooks.Count();

                if (pageSize > 0)
                {
                    document.lstEBooks = document.lstEBooks.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadEBook()
        {
            if (Session["User"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        [HttpPost]
        public ActionResult UploadEBook(HttpPostedFileBase file, EBooksIndex eBook)
        {            
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    if (file.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(file.FileName);
                        string _path = Path.Combine(Server.MapPath("~/EBooks"), _FileName);
                        string FilePath = "EBooks/" + _FileName;
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(eBook);
                        }
                        else if (db.EBooksIndexes.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            file.SaveAs(_path);
                            eBook.FilePath = FilePath;
                            eBook.IsActive = true;
                            eBook.CreatedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.EBooksIndexes.Add(eBook);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(eBook);                            
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(eBook);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(eBook);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(eBook);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadEBook(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new EBooksDownloadDetail();
                objDoc.BooKId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.EBooksDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.EBooksIndexes.Where(x => x.IsActive == true & x.BooKId == id).SingleOrDefault();

                string path = AppDomain.CurrentDomain.BaseDirectory + objDocument.FilePath;
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = System.IO.Path.GetFileName(path);
                return File(fileBytes, "application/unknown", fileName);

            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
