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
    public class PrayerNoteController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index(int page = 1, string search = "")
        {
            List<PrayerNoteVM> lstPrayerNoteDownloadDetail = new List<PrayerNoteVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetPrayerNotes(search, skip, pageSize, out totalRecord);                
                foreach (var prayerNote in data.lstPrayerNotes)
                {
                    List<string> lstPrayerNoteDownloadedUsers = db.PrayerNoteDownloadDetails.Where(pn => pn.PrayerNoteId == prayerNote.PrayerNoteId).Select(pn => pn.DownloadedBy).Distinct().ToList();
                    List<PrayerNoteDownloadDetail> lstPrayerNoteDownloadDetails = new List<PrayerNoteDownloadDetail>();
                    foreach (string user in lstPrayerNoteDownloadedUsers)
                    {
                        lstPrayerNoteDownloadDetails.Add(new PrayerNoteDownloadDetail { DownloadedBy = user });
                    }
                    prayerNote.totalDownloadedCount = lstPrayerNoteDownloadedUsers.Count();
                    lstPrayerNoteDownloadDetail.Add(new PrayerNoteVM { prayerNote = prayerNote, lstPrayerNoteDownloadDetails = lstPrayerNoteDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstPrayerNoteDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetPrayerNotes(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstPrayerNotes = db.PrayerNotes.Where(p => p.Title.Contains(search)).Where(p => p.IsActive == true).ToList();
                document.lstPrayerNotes = document.lstPrayerNotes.OrderByDescending(p => p.PrayerDate).ToList();
                totalRecord = document.lstPrayerNotes.Count();

                if (pageSize > 0)
                {
                    document.lstPrayerNotes = document.lstPrayerNotes.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadPrayerNote()
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
        public ActionResult UploadPrayerNote(DocumentViewModels prayerNote, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/PrayerNotes/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(prayerNote);
                        }
                        else if (db.PrayerNotes.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/PrayerNotes"), fileName);
                            file.SaveAs(path);
                            var objPrayerNote = new PrayerNote();
                            objPrayerNote.Title = prayerNote.Title;
                            objPrayerNote.FilePath = FilePath;
                            objPrayerNote.PrayerDate = Convert.ToDateTime(prayerNote.PrayerDate.ToString("dd-MM-yyyy"));
                            objPrayerNote.CreatedBy = Session["UserType"].ToString();
                            objPrayerNote.CreatedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            objPrayerNote.IsActive = true;
                            db.PrayerNotes.Add(objPrayerNote);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(prayerNote);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(prayerNote);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(prayerNote);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(prayerNote);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadPrayerNote(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objPrayerNote = new PrayerNoteDownloadDetail();
                objPrayerNote.PrayerNoteId = id;
                objPrayerNote.DownloadedBy = Session["UserEmail"].ToString();
                objPrayerNote.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objPrayerNote.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objPrayerNote.UserId = Convert.ToInt32(Session["UserId"]);
                db.PrayerNoteDownloadDetails.Add(objPrayerNote);
                db.SaveChanges();

                var objPrayerNoteDocument = db.PrayerNotes.Where(x => x.IsActive == true & x.PrayerNoteId == id).SingleOrDefault();
                if (!string.IsNullOrEmpty(objPrayerNoteDocument.FilePath))
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + objPrayerNoteDocument.FilePath;
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    string fileName = System.IO.Path.GetFileName(path);
                    return File(fileBytes, "application/unknown", fileName);
                }
                else
                {
                    return RedirectToAction("Index", "SabbathDay");
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
