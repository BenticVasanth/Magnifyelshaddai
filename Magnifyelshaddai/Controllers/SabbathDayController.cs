using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class SabbathDayController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Dashboard()
        {
            if (Session["User"] != null)
            {
                ViewBag.SabbathDayDocumentsCount = db.Documents.Where(s => s.Status == true).ToList().Count();
                ViewBag.VirginBiblioDocumentsCount = db.LibraryIndexes.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.StudyBiblesCount = db.StudyBibles.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.BibleCommentariesCount = db.BibleCommentaries.Where(b => b.IsActive == true).ToList().Count();
                ViewBag.BiblicalTheologiesCount = db.BiblicalTheologies.Where(b => b.IsActive == true).ToList().Count();
                ViewBag.BibleDictionariesCount = db.BibleDictionaries.Where(b => b.IsActive == true).ToList().Count();
                ViewBag.EBooksCount = db.EBooksIndexes.Where(e => e.IsActive == true).Count();
                ViewBag.SpiritualArticlesCount = db.SpiritualArticles.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.MarianBooksCount = db.MarianBooks.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.MarianStudiesCount = db.MarianStudies.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.PrayerRainCount = db.PrayerRains.Where(p => p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true).ToList().Count();
                ViewBag.PrayerNotesCount = db.PrayerNotes.Where(s => s.IsActive == true).ToList().Count();
                ViewBag.SinaiLettersCount = db.SinaiLetters.Where(s => s.IsActive == true).ToList().Count();

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        //Lofgged In details
        public ActionResult LoggedInDetails()
        {
            List<LoggedInDetailVM> lstLoggedInDetail = new List<LoggedInDetailVM>();

            if (Session["User"] != null)
            {
                List<DateTime?> lstLoggedInDates = db.LoggedInDetails.Select(l => l.LastLoggedInDateAndTime).ToList();
                List<DateTime?> lstFormattedLoggedInDates = new List<DateTime?>();
                foreach (DateTime loggedInDate in lstLoggedInDates)
                {
                    DateTime formattedLoggedInDate = Convert.ToDateTime(loggedInDate.ToString("yyyy-MM-dd"));
                    lstFormattedLoggedInDates.Add(formattedLoggedInDate);
                }
                lstLoggedInDates = lstFormattedLoggedInDates.Distinct().ToList();
                lstLoggedInDates = lstLoggedInDates.OrderByDescending(l => l.Value).ToList();

                foreach (var loggedInDate in lstLoggedInDates)
                {
                    List<LoggedInDetail> lstLoggedInDetails = db.LoggedInDetails.Where(l => EntityFunctions.TruncateTime(l.LastLoggedInDateAndTime) == loggedInDate).ToList();
                    List<LoggedInDetail> lstLoggedInData = new List<LoggedInDetail>();
                    foreach (LoggedInDetail loggedInDetail in lstLoggedInDetails)
                    {
                        lstLoggedInData.Add(loggedInDetail);
                    }

                    LoggedInDetail obj = new LoggedInDetail();
                    obj.LastLoggedInDateAndTime = loggedInDate;
                    obj.totalNoOfUsersLoggedInPerDay = lstLoggedInData.Count();

                    lstLoggedInDetail.Add(new LoggedInDetailVM { loggedInDetail = obj, lstLoggedInDetails = lstLoggedInData });
                }
                ViewBag.TotalRows = lstLoggedInDates.Count();
                return View(lstLoggedInDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult Index(int page = 1, string search = "")
        {
            List<SabbathDayDocumentVM> lstDownloadDetail = new List<SabbathDayDocumentVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetSabbathDayDocuments(search, skip, pageSize, out totalRecord);
                foreach (var sabbathDayDocument in data.lstSabbathDayDocuments)
                {
                    List<string> lstSabbathDayDocumentDownloadedUsers = db.DownloadDetails.Where(d => d.DocumentId == sabbathDayDocument.IntId).Select(s => s.DownloadedBy).Distinct().ToList();
                    List<DownloadDetail> lstDownloadDetails = new List<DownloadDetail>();
                    foreach (string user in lstSabbathDayDocumentDownloadedUsers)
                    {
                        lstDownloadDetails.Add(new DownloadDetail { DownloadedBy = user });
                    }
                    sabbathDayDocument.totalDownloadedCount = lstSabbathDayDocumentDownloadedUsers.Count();
                    lstDownloadDetail.Add(new SabbathDayDocumentVM { sabbathDayDocument = sabbathDayDocument, lstSabbathDayDocumentsDownloadDetails = lstDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetSabbathDayDocuments(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstSabbathDayDocuments = db.Documents.Where(s => s.Title.Contains(search) || s.DocumentReference.Contains(search)).Where(s => s.Status == true).OrderByDescending(s => s.IntId).ToList();
                totalRecord = document.lstSabbathDayDocuments.Count();

                if (pageSize > 0)
                {
                    document.lstSabbathDayDocuments = document.lstSabbathDayDocuments.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        public ActionResult UploadSabbathDayDocument()
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
        public ActionResult UploadSabbathDayDocument(HttpPostedFileBase file, DocumentViewModels Doc)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Admin")
                {
                    if (file.ContentLength > 0)
                    {
                        string _FileName = Path.GetFileName(file.FileName);
                        string _path = Path.Combine(Server.MapPath("../Documents/SabbathDayMeditation"), _FileName);
                        string FilePath = "Documents/SabbathDayMeditation/" + _FileName;
                        string AllowedFileExtensions = ".pdf";
                        if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                        {
                            ViewBag.Message = "The file type should be .pdf";
                            return View(Doc);
                        }
                        else if (db.Documents.Where(x => x.FilePath == FilePath).Count() == 0)
                        {
                            file.SaveAs(_path);
                            var objDoc = new Document();
                            objDoc.CategoryId = 1;
                            objDoc.Title = Doc.Title;
                            objDoc.FilePath = FilePath;
                            objDoc.ImagesPath = Doc.ImagesPath;
                            objDoc.Description = Doc.Description;
                            objDoc.DocumentReference = Doc.DocumentReference;
                            objDoc.CreatedBy = Session["UserType"].ToString();
                            objDoc.CreatedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            objDoc.Status = true;
                            db.Documents.Add(objDoc);
                            db.SaveChanges();

                            //Email Send all
                            EmailModel obj = new EmailModel();
                            var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                            obj.ToEmail = "bibleworkshopteam@magnifyelshaddai.com";
                            obj.EmailSubject = "Bible Workshop Team - Sabbath Day document";
                            string Email = "<P>Praise the LORD Brother, <br/><br/> Greetings in the name of LORD Jesus Christ and Mother Mary.<br/>" +
                            "<b>Bible Workshop Team</b> uploaded the <b>Sabbath day document</b> in <a href='http://www.magnifyelshaddai.com/' target='_blank'>http://www.<wbr>magnifyelshaddai.com/</a>" +
                            "<br/><b><i><font color='#0b5394'>Glory to the LORD Jesus Christ and Mother Mary.</font></i></b><br/><br/>" +
                            "<b><i><font color='#0b5394'>In Christ,</font></i></b><br/>" +
                            "<b><i><font color='#0b5394'>Bible Workshop Team.</font></i></b><br/></p>";
                            obj.EMailBody = Email;
                            obj.EmailCC = "";

                            var emailList = db.Users.Where(x => x.UserId != 1 && x.IsActive == true && x.IsNotification == true).Select(u => u.Email).ToList();
                            //var emailList = db.Users.Where(x => x.Email.Equals("edwin@magnifyelshaddai.com")).Select(u => u.Email).ToList();

                            var emails = String.Join(",", emailList);

                            obj.EmailBCC = emails.ToString();

                            using (SmtpClient smtpClient = new SmtpClient())
                            {
                                smtpClient.Host = "relay-hosting.secureserver.net";
                                //smtpClient.Host = "smtp.gmail.com";
                                smtpClient.Port = 25;
                                smtpClient.EnableSsl = false;
                                MailMessage message = new MailMessage();
                                MailAddress fromAddress = new MailAddress(EmailDB.EmailID);
                                MailAddress toAddress = new MailAddress(obj.ToEmail);
                                smtpClient.Credentials = new System.Net.NetworkCredential(EmailDB.EmailID, EmailDB.EmailPassword);
                                message.From = fromAddress;
                                message.To.Add(toAddress);
                                message.Bcc.Add(obj.EmailBCC);
                                message.IsBodyHtml = true;
                                message.Subject = obj.EmailSubject;
                                message.Body = obj.EMailBody;
                                smtpClient.Send(message);
                                ViewBag.SuccessMessage = "Email Sent Successfully.";
                            }

                            //SmtpClient smtpClient = new SmtpClient();
                            //smtpClient.Host = "smtp.gmail.com";
                            //smtpClient.Port = 25;
                            //smtpClient.EnableSsl = false;
                            //MailMessage message = new MailMessage();
                            //MailAddress fromAddress = new MailAddress(EmailDB.EmailID);
                            //MailAddress toAddress = new MailAddress(obj.ToEmail);
                            //smtpClient.Credentials = new System.Net.NetworkCredential(EmailDB.EmailID, EmailDB.EmailPassword);
                            //message.From = fromAddress;
                            //message.To.Add(toAddress);
                            //message.Bcc.Add(obj.EmailBCC);
                            //message.IsBodyHtml = true;
                            //message.Subject = obj.EmailSubject;
                            //message.Body = obj.EMailBody;
                            //smtpClient.Send(message);

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully.";
                            return View(Doc);
                        }
                        else
                        {
                            ModelState.Clear();
                            ViewBag.Message = "This Document is already uploaded.";
                            return View(Doc);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(Doc);
                    }
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

        public ActionResult DownloadSabbathDayDocument(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new DownloadDetail();
                objDoc.DocumentId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.DownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.Documents.Where(x => x.Status == true & x.IntId == id).SingleOrDefault();

                string path = AppDomain.CurrentDomain.BaseDirectory + objDocument.FilePath;
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = System.IO.Path.GetFileName(path);
                return File(fileBytes, "application/pdf", fileName);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
