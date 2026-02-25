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
    public class VirginBiblioController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                var doc = db.LibraryIndexes.OrderByDescending(x => x.BooKId).Take(3).ToList();
                ViewBag.LatestDocs = doc;
                var libraryIndex = new DocumentView
                {
                    lstLibraryBooks = db.LibraryIndexes.Where(c => c.IsActive == true).OrderByDescending(c => c.BooKId).ToList()
                };
                ViewBag.Document = libraryIndex;
                ViewBag.UserType = Session["UserType"].ToString();
                return View(libraryIndex);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult UploadVirginBiblioBook()
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
        public ActionResult UploadVirginBiblioBook(LibraryIndex library, HttpPostedFileBase UpladBook)
        {
            if (Session["User"] != null)
            {
                if (UpladBook == null)
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
                else if (UpladBook.ContentLength > 0)
                {
                    int MaxContentLength = 1024 * 1024 * 20; //3 MB
                    string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf" };

                    if (!AllowedFileExtensions.Contains(UpladBook.FileName.Substring(UpladBook.FileName.LastIndexOf('.'))))
                    {
                        ModelState.AddModelError("File", "Please file of type: " + string.Join(", ", AllowedFileExtensions));
                    }

                    else if (UpladBook.ContentLength > MaxContentLength)
                    {
                        ModelState.AddModelError("File", "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB");
                    }
                    else
                    {
                        //TO:DO
                        var fileName = Path.GetFileName(UpladBook.FileName);
                        var path = Path.Combine(Server.MapPath("~/Library"), fileName);
                        UpladBook.SaveAs(path);
                        //library.FilePath = "~/Library" + fileName;
                        library.FilePath = "Library" + "/" + fileName;
                        library.IsActive = true;
                        db.LibraryIndexes.Add(library);
                        db.SaveChanges();
                        ModelState.Clear();
                        ViewBag.Message = "File uploaded successfully";

                        //Email Send all                               

                        EmailModel obj = new EmailModel();

                        var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                        obj.ToEmail = "bibleworkshopteam@magnifyelshaddai.com";
                        obj.EmailSubject = "Bible Workshop Team";

                        string Email = "<P>Praise the LORD Brother,\n\n Greetings in the name of LORD Jesus Christ and Mother Mary.\n" +
                        "<b>Bible workshop team</b> uploaded the Virgin Biblio in <a href='http://www.magnifyelshaddai.com/' target='_blank'>http://www.<wbr>magnifyelshaddai.com/</a>" +
                        "</br>\n\n<b><i><font color='#0b5394'>Glory to the LORD Jesus Christ and Mother Mary.</font></i></b>\n\n" +
                        "<b><i><font color='#0b5394'>In Christ,</font></i></b>\n" +
                        "<b><i><font color='#0b5394'>Bible Workshop Team.</font></i></b>\n</p>";
                        obj.EMailBody = Email;
                        obj.EmailCC = "";

                        var emailList = db.Users.Where(x => x.UserId != 1 && x.IsActive == true && x.IsNotification == true).Select(u => u.Email).ToList();

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
                    }
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadVirginBiblioDocument(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                var objDoc = new LibrayDownloadDetail();
                objDoc.BooKId = id;
                objDoc.DownloadedBy = Session["UserEmail"].ToString();
                objDoc.DownloadedDateTime = DateTime.Now;
                objDoc.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objDoc.UserId = Convert.ToInt32(Session["UserId"]);
                db.LibrayDownloadDetails.Add(objDoc);
                db.SaveChanges();

                var objDocument = db.LibraryIndexes.Where(x => x.IsActive == true & x.BooKId == id).SingleOrDefault();

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

        [HttpPost]
        public ActionResult VirginBiblioDocumentSearch(string txtfilter)
        {
            if (Session["User"] != null)
            {
                Session["txtfilter"] = txtfilter;
                List<LibraryIndex> libraDocument = new List<LibraryIndex>();
                if (!string.IsNullOrWhiteSpace(txtfilter))
                {

                    libraDocument = db.LibraryIndexes.Where(p => p.Title.Contains(txtfilter) || p.Author.Contains(txtfilter)).Where(p => p.IsActive == true).OrderByDescending(p => p.BooKId).ToList();
                }
                else
                {
                    libraDocument = db.LibraryIndexes.Where(p => p.IsActive == true).OrderByDescending(p => p.BooKId).ToList();
                    return PartialView("_VirginBiblio", libraDocument);
                }

                return PartialView("_VirginBiblio", libraDocument);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
