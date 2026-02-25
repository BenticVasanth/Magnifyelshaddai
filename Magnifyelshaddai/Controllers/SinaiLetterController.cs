using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class SinaiLetterController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        string currentDateAndTime = (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE)).ToString("ddMMMyyyyHHmmss");

        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                List<SinaiLetter> lstLatestSinaiLetter = new List<SinaiLetter>();
                lstLatestSinaiLetter = db.SinaiLetters.OrderByDescending(x => x.Id).Take(3).ToList();
                ViewBag.LatestSinaiLetters = lstLatestSinaiLetter;

                List<SinaiLetter> lstSinaiLetter = new List<SinaiLetter>();
                lstSinaiLetter = db.SinaiLetters.Where(c => c.IsActive == true).OrderByDescending(c => c.Id).ToList();
                ViewBag.SinaiLetters = lstSinaiLetter;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult UploadSinaiLetter()
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
        public ActionResult UploadSinaiLetter(HttpPostedFileBase file, DocumentViewModels Doc)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "SuperUser")
                {
                    string oldFileName = Path.GetFileName(file.FileName);
                    string oldFileExtention = Path.GetExtension(file.FileName);                   

                    int sinaiLetterIDToBeCreated;
                    if (db.SinaiLetters.Count() > 0)
                    {
                        sinaiLetterIDToBeCreated = db.SinaiLetters.Max(s => s.Id);
                    }
                    else
                    {
                        sinaiLetterIDToBeCreated = 0;
                    }

                    sinaiLetterIDToBeCreated = sinaiLetterIDToBeCreated + 1;
                    string finalFileName = "SinaiLetter" + sinaiLetterIDToBeCreated + "_" + currentDateAndTime + oldFileExtention;

                    string SinaiLetterUploadPath = Path.Combine(Server.MapPath("../Documents/SinaiLetters"), finalFileName);
                    file.SaveAs(SinaiLetterUploadPath);

                    var sinaiLetter = new SinaiLetter();
                    sinaiLetter.FilePath = "Documents/SinaiLetters/" + finalFileName;
                    sinaiLetter.IsActive = true;
                    sinaiLetter.CreatedBy = Convert.ToInt32(Session["UserId"]);
                    sinaiLetter.CreatedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    db.SinaiLetters.Add(sinaiLetter);
                    db.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = "File uploaded successfully.";
                    return View(sinaiLetter);                 
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View(Doc);
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadSinaiLetter(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objSinaiLetter = new SinaiLetterDownloadDetail();
                objSinaiLetter.SinaiLetterId = id;
                objSinaiLetter.DownloadedBy = Session["UserEmail"].ToString();
                objSinaiLetter.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objSinaiLetter.DownloadedIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objSinaiLetter.UserId = Convert.ToInt32(Session["UserId"]);
                db.SinaiLetterDownloadDetails.Add(objSinaiLetter);
                db.SaveChanges();

                var objSinaiLetterImage = db.SinaiLetters.Where(x => x.IsActive == true & x.Id == id).SingleOrDefault();
                if (!string.IsNullOrEmpty(objSinaiLetterImage.FilePath))
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + objSinaiLetterImage.FilePath;
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    string fileName = System.IO.Path.GetFileName(path);
                    return File(fileBytes, "application/unknown");
                }
                else
                {
                    return RedirectToAction("Index", "SinaiLetter");
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
