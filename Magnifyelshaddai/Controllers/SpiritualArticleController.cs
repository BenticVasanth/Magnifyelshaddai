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
    public class SpiritualArticleController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index(int page = 1, string search = "")
        {
            List<SpiritualArticleVM> lstSpiritualArticleDownloadDetail = new List<SpiritualArticleVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetSpiritualArticles(search, skip, pageSize, out totalRecord);
                foreach (var spiritualArticle in data.lstSpiritualArticles)
                {
                    List<string> lstSpiritualArticleDownloadedUsers = db.SpiritualArticleDownloadDetails.Where(sa => sa.SpiritualArticleId == spiritualArticle.SpiritualArticleId).Select(sa => sa.DownloadedBy).Distinct().ToList();
                    List<SpiritualArticleDownloadDetail> lstSpiritualArticleDownloadDetails = new List<SpiritualArticleDownloadDetail>();
                    foreach (string user in lstSpiritualArticleDownloadedUsers)
                    {
                        lstSpiritualArticleDownloadDetails.Add(new SpiritualArticleDownloadDetail { DownloadedBy = user });
                    }
                    spiritualArticle.totalDownloadedCount = lstSpiritualArticleDownloadedUsers.Count();
                    lstSpiritualArticleDownloadDetail.Add(new SpiritualArticleVM { spiritualArticle = spiritualArticle, lstSpiritualArticleDownloadDetails = lstSpiritualArticleDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstSpiritualArticleDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetSpiritualArticles(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstSpiritualArticles = db.SpiritualArticles.Where(s => s.Title.Contains(search) || s.Author.Contains(search)).Where(s => s.IsActive == true).OrderByDescending(e => e.SpiritualArticleId).ToList();
                totalRecord = document.lstSpiritualArticles.Count();

                if (pageSize > 0)
                {
                    document.lstSpiritualArticles = document.lstSpiritualArticles.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadSpiritualArticle()
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
        public ActionResult UploadSpiritualArticle(SpiritualArticle spiritualArticle, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    if (file.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(file.FileName);
                        string _path = Path.Combine(Server.MapPath("~/Documents/SpiritualArticles"), _FileName);
                        string FilePath = "Documents/SpiritualArticles/" + _FileName;
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(spiritualArticle);
                        }
                        else if (db.SpiritualArticles.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            file.SaveAs(_path);
                            spiritualArticle.FilePath = FilePath;
                            spiritualArticle.IsActive = true;
                            spiritualArticle.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            db.SpiritualArticles.Add(spiritualArticle);
                            db.SaveChanges();

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(spiritualArticle);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(spiritualArticle);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(spiritualArticle);
                    }
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(spiritualArticle);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadSpiritualArticle(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new SpiritualArticleDownloadDetail();
                objDoc.SpiritualArticleId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.SpiritualArticleDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.SpiritualArticles.Where(x => x.IsActive == true & x.SpiritualArticleId == id).SingleOrDefault();

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
