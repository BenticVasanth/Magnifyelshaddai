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
    public class MariologyController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult MarianBook(int page = 1, string search = "")
        {
            List<MarianBookVM> lstMarianBookDownloadDetail = new List<MarianBookVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetMarianBook(search, skip, pageSize, out totalRecord);
                foreach (var marianBook in data.lstMarianBooks)
                {
                    List<string> lstMarianBookDownloadedUsers = db.MarianBookDownloadDetails.Where(mb => mb.MarianBookId == marianBook.MarianBookId).Select(mb => mb.DownloadedBy).Distinct().ToList();
                    List<MarianBookDownloadDetail> lstMarianBookDownloadDetails = new List<MarianBookDownloadDetail>();
                    foreach (string user in lstMarianBookDownloadedUsers)
                    {
                        lstMarianBookDownloadDetails.Add(new MarianBookDownloadDetail { DownloadedBy = user });
                    }
                    marianBook.totalDownloadedCount = lstMarianBookDownloadedUsers.Count();
                    lstMarianBookDownloadDetail.Add(new MarianBookVM { marianBook = marianBook, lstMarianBookDownloadDetails = lstMarianBookDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstMarianBookDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetMarianBook(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstMarianBooks = db.MarianBooks.Where(m => m.Title.Contains(search) || m.Author.Contains(search)).Where(m => m.IsActive == true).OrderByDescending(m => m.MarianBookId).ToList();
                totalRecord = document.lstMarianBooks.Count();

                if (pageSize > 0)
                {
                    document.lstMarianBooks = document.lstMarianBooks.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadMarianBook()
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
        public ActionResult UploadMarianBook(MarianBook marianBook, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Mariology/MarianBooks/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(marianBook);
                        }
                        else if (db.MarianBooks.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Mariology/MarianBooks"), fileName);
                            file.SaveAs(path);
                            marianBook.FilePath = "Documents/Mariology/MarianBooks" + "/" + fileName;
                            marianBook.IsActive = true;
                            marianBook.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.MarianBooks.Add(marianBook);
                            db.SaveChanges();
                            
                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(marianBook);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(marianBook);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(marianBook);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(marianBook);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadMarianBook(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new MarianBookDownloadDetail();
                objDoc.MarianBookId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.MarianBookDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.MarianBooks.Where(x => x.IsActive == true & x.MarianBookId == id).SingleOrDefault();

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

        public ActionResult MarianStudy(int page = 1, string search = "")
        {
            List<MarianStudyVM> lstMarianStudyDownloadDetail = new List<MarianStudyVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetMarianStudy(search, skip, pageSize, out totalRecord);
                foreach (var marianStudy in data.lstMarianStudies)
                {
                    List<string> lstMarianStudyDownloadedUsers = db.MarianStudyDownloadDetails.Where(ms => ms.MarianStudyId == marianStudy.MarianStudyId).Select(ms => ms.DownloadedBy).Distinct().ToList();
                    List<MarianStudyDownloadDetail> lstMarianStudyDownloadDetails = new List<MarianStudyDownloadDetail>();
                    foreach (string user in lstMarianStudyDownloadedUsers)
                    {
                        lstMarianStudyDownloadDetails.Add(new MarianStudyDownloadDetail { DownloadedBy = user });
                    }
                    marianStudy.totalDownloadedCount = lstMarianStudyDownloadedUsers.Count();
                    lstMarianStudyDownloadDetail.Add(new MarianStudyVM { marianStudy = marianStudy, lstMarianStudyDownloadDetails = lstMarianStudyDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstMarianStudyDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetMarianStudy(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstMarianStudies = db.MarianStudies.Where(m => m.Title.Contains(search) || m.Author.Contains(search)).Where(m => m.IsActive == true).OrderByDescending(m => m.MarianStudyId).ToList();
                totalRecord = document.lstMarianStudies.Count();

                if (pageSize > 0)
                {
                    document.lstMarianStudies = document.lstMarianStudies.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadMarianStudy()
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
        public ActionResult UploadMarianStudy(MarianStudy marianStudy, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Mariology/MarianStudies/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(marianStudy);
                        }
                        else if (db.MarianStudies.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Mariology/MarianStudies"), fileName);
                            file.SaveAs(path);
                            marianStudy.FilePath = "Documents/Mariology/MarianStudies/" + fileName;
                            marianStudy.IsActive = true;
                            marianStudy.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.MarianStudies.Add(marianStudy);
                            db.SaveChanges();
                            
                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(marianStudy);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(marianStudy);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(marianStudy);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(marianStudy);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadMarianStudy(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new MarianStudyDownloadDetail();
                objDoc.MarianStudyId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.MarianStudyDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.MarianStudies.Where(x => x.IsActive == true & x.MarianStudyId == id).SingleOrDefault();

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
