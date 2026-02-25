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
    public class BibleController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index()
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

        public ActionResult StudyBible(int page = 1, string search = "")
        {
            List<StudyBibleVM> lstStudyBibleDownloadDetail = new List<StudyBibleVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetStudyBibles(search, skip, pageSize, out totalRecord);
                foreach (var studyBible in data.lstStudyBibles)
                {
                    List<string> lstStudyBibleDownloadedUsers = db.StudyBibleDownloadDetails.Where(s => s.StudyBibleId == studyBible.StudyBibleId).Select(sb => sb.DownloadedBy).Distinct().ToList();
                    List<StudyBibleDownloadDetail> lstStudyBibleDownloadDetails = new List<StudyBibleDownloadDetail>();
                    foreach (string user in lstStudyBibleDownloadedUsers)
                    {
                        lstStudyBibleDownloadDetails.Add(new StudyBibleDownloadDetail { DownloadedBy = user });
                    }
                    studyBible.totalDownloadedCount = lstStudyBibleDownloadedUsers.Count();
                    lstStudyBibleDownloadDetail.Add(new StudyBibleVM { studyBible = studyBible, lstStudyBibleDownloadDetails = lstStudyBibleDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstStudyBibleDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetStudyBibles(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstStudyBibles = db.StudyBibles.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.StudyBibleId).ToList();
                totalRecord = document.lstStudyBibles.Count();

                if (pageSize > 0)
                {
                    document.lstStudyBibles = document.lstStudyBibles.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadStudyBible()
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
        public ActionResult UploadStudyBible(StudyBible studyBible, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Bible/StudyBibles/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(studyBible);
                        }
                        else if (db.StudyBibles.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Bible/StudyBibles"), fileName);
                            file.SaveAs(path);
                            studyBible.FilePath = "Documents/Bible/StudyBibles/" + fileName;
                            studyBible.IsActive = true;
                            studyBible.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.StudyBibles.Add(studyBible);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(studyBible);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(studyBible);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(studyBible);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(studyBible);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadStudyBible(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new StudyBibleDownloadDetail();
                objDoc.StudyBibleId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadedIP = ipAddress;
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.StudyBibleDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.StudyBibles.Where(x => x.IsActive == true & x.StudyBibleId == id).SingleOrDefault();

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

        public ActionResult BibleCommentary(int page = 1, string search = "")
        {
            List<BibleCommentaryVM> lstBibleCommentaryDownloadDetail = new List<BibleCommentaryVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetBibleCommentaries(search, skip, pageSize, out totalRecord);
                foreach (var bibleCommentary in data.lstBibleCommentaries)
                {
                    List<string> lstBibleCommentaryDownloadedUsers = db.BibleCommentaryDownloadDetails.Where(bc => bc.BibleCommentary == bibleCommentary.BibleCommentaryId).Select(bc => bc.DownloadedBy).Distinct().ToList();
                    List<BibleCommentaryDownloadDetail> lstBibleCommentaryDownloadDetails = new List<BibleCommentaryDownloadDetail>();
                    foreach (string user in lstBibleCommentaryDownloadedUsers)
                    {
                        lstBibleCommentaryDownloadDetails.Add(new BibleCommentaryDownloadDetail { DownloadedBy = user });
                    }
                    bibleCommentary.totalDownloadedCount = lstBibleCommentaryDownloadedUsers.Count();
                    lstBibleCommentaryDownloadDetail.Add(new BibleCommentaryVM { bibleCommentary = bibleCommentary, lstBibleCommentaryDownloadDetails = lstBibleCommentaryDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstBibleCommentaryDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetBibleCommentaries(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstBibleCommentaries = db.BibleCommentaries.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BibleCommentaryId).ToList();
                totalRecord = document.lstBibleCommentaries.Count();

                if (pageSize > 0)
                {
                    document.lstBibleCommentaries = document.lstBibleCommentaries.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadBibleCommentary()
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
        public ActionResult UploadBibleCommentary(BibleCommentary bibleCommentary, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Bible/BibleCommentaries/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(bibleCommentary);
                        }
                        else if (db.BibleCommentaries.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Bible/BibleCommentaries"), fileName);
                            file.SaveAs(path);
                            bibleCommentary.FilePath = "Documents/Bible/BibleCommentaries/" + fileName;
                            bibleCommentary.IsActive = true;
                            bibleCommentary.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.BibleCommentaries.Add(bibleCommentary);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(bibleCommentary);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(bibleCommentary);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(bibleCommentary);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(bibleCommentary);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadBibleCommentary(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new BibleCommentaryDownloadDetail();
                objDoc.BibleCommentary = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadedIP = ipAddress;
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.BibleCommentaryDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.BibleCommentaries.Where(x => x.IsActive == true & x.BibleCommentaryId == id).SingleOrDefault();

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

        public ActionResult BiblicalTheology(int page = 1, string search = "")
        {
            List<BiblicalTheologyVM> lstBiblicalTheologyDownloadDetail = new List<BiblicalTheologyVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetBiblicalTheology(search, skip, pageSize, out totalRecord);
                foreach (var biblicalTheology in data.lstBiblicalTheologies)
                {
                    List<string> lstBiblicalTheologyDownloadedUsers = db.BiblicalTheologyDownloadDetails.Where(bt => bt.BiblicalTheologyId == biblicalTheology.BiblicalTheologyId).Select(bt => bt.DownloadedBy).Distinct().ToList();
                    List<BiblicalTheologyDownloadDetail> lstBiblicalTheologyDownloadDetails = new List<BiblicalTheologyDownloadDetail>();
                    foreach (string user in lstBiblicalTheologyDownloadedUsers)
                    {
                        lstBiblicalTheologyDownloadDetails.Add(new BiblicalTheologyDownloadDetail { DownloadedBy = user });
                    }
                    biblicalTheology.totalDownloadedCount = lstBiblicalTheologyDownloadedUsers.Count();
                    lstBiblicalTheologyDownloadDetail.Add(new BiblicalTheologyVM { biblicalTheology = biblicalTheology, lstBiblicalTheologyDownloadDetails = lstBiblicalTheologyDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstBiblicalTheologyDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetBiblicalTheology(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstBiblicalTheologies = db.BiblicalTheologies.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BiblicalTheologyId).ToList();
                totalRecord = document.lstBiblicalTheologies.Count();

                if (pageSize > 0)
                {
                    document.lstBiblicalTheologies = document.lstBiblicalTheologies.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadBiblicalTheology()
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
        public ActionResult UploadBiblicalTheology(BiblicalTheology biblicalTheology, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Bible/BiblicalTheologies/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(biblicalTheology);
                        }
                        else if (db.BiblicalTheologies.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Bible/BiblicalTheologies"), fileName);
                            file.SaveAs(path);
                            biblicalTheology.FilePath = "Documents/Bible/BiblicalTheologies/" + fileName;
                            biblicalTheology.IsActive = true;
                            biblicalTheology.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.BiblicalTheologies.Add(biblicalTheology);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(biblicalTheology);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(biblicalTheology);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(biblicalTheology);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(biblicalTheology);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadBiblicalTheology(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new BiblicalTheologyDownloadDetail();
                objDoc.BiblicalTheologyId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadedIP = ipAddress;
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.BiblicalTheologyDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.BiblicalTheologies.Where(x => x.IsActive == true & x.BiblicalTheologyId == id).SingleOrDefault();

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

        public ActionResult BibleDictionary(int page = 1, string search = "")
        {
            List<BibleDictionaryVM> lstBibleDictionaryDownloadDetail = new List<BibleDictionaryVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetBibleDictionary(search, skip, pageSize, out totalRecord);
                foreach (var bibleDictionary in data.lstBibleDictionaries)
                {
                    List<string> lstBibleDictionaryDownloadedUsers = db.BibleDictionaryDownloadDetails.Where(bd => bd.BibleDictionaryId == bibleDictionary.BibleDictionaryId).Select(bd => bd.DownloadedBy).Distinct().ToList();
                    List<BibleDictionaryDownloadDetail> lstBibleDictionaryDownloadDetails = new List<BibleDictionaryDownloadDetail>();
                    foreach (string user in lstBibleDictionaryDownloadedUsers)
                    {
                        lstBibleDictionaryDownloadDetails.Add(new BibleDictionaryDownloadDetail { DownloadedBy = user });
                    }
                    bibleDictionary.totalDownloadedCount = lstBibleDictionaryDownloadedUsers.Count();
                    lstBibleDictionaryDownloadDetail.Add(new BibleDictionaryVM { bibleDictionary = bibleDictionary, lstBibleDictionaryDownloadDetails = lstBibleDictionaryDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstBibleDictionaryDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetBibleDictionary(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstBibleDictionaries = db.BibleDictionaries.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BibleDictionaryId).ToList();
                totalRecord = document.lstBibleDictionaries.Count();

                if (pageSize > 0)
                {
                    document.lstBibleDictionaries = document.lstBibleDictionaries.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadBibleDictionary()
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
        public ActionResult UploadBibleDictionary(BibleDictionary bibleDictionary, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string FilePath = "Documents/Bible/BibleDictionaries/" + _FileName;
                    if (file.ContentLength > 0)
                    {
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(bibleDictionary);
                        }
                        else if (db.BibleDictionaries.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Documents/Bible/BibleDictionaries"), fileName);
                            file.SaveAs(path);
                            bibleDictionary.FilePath = "Documents/Bible/BibleDictionaries/" + fileName;
                            bibleDictionary.IsActive = true;
                            bibleDictionary.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.BibleDictionaries.Add(bibleDictionary);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(bibleDictionary);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(bibleDictionary);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(bibleDictionary);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(bibleDictionary);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadBibleDictionary(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new BibleDictionaryDownloadDetail();
                objDoc.BibleDictionaryId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadedIP = ipAddress;
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.BibleDictionaryDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.BibleDictionaries.Where(x => x.IsActive == true & x.BibleDictionaryId == id).SingleOrDefault();

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
