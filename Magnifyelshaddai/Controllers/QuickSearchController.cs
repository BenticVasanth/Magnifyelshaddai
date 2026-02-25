using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class QuickSearchController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public ActionResult Index(string search)
        {
            if (Session["User"] != null)
            {
                DocumentView doc = GetDocuments(search);
                ViewBag.search = search;
                return View(doc);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetDocuments(string search)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                ViewBag.lstSabbathDayDocuments = db.Documents.Where(s => s.Title.Contains(search) || s.DocumentReference.Contains(search)).Where(s => s.Status == true).OrderByDescending(s => s.IntId).ToList();
                ViewBag.lstStudyBibles = db.StudyBibles.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.StudyBibleId).ToList();
                ViewBag.lstBibleCommentaries = db.BibleCommentaries.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BibleCommentaryId).ToList();
                ViewBag.lstBiblicalTheologies = db.BiblicalTheologies.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BiblicalTheologyId).ToList();
                ViewBag.lstBibleDictionaries = db.BibleDictionaries.Where(p => p.Title.Contains(search) || p.Author.Contains(search)).Where(p => p.IsActive == true).OrderByDescending(p => p.BibleDictionaryId).ToList();
                ViewBag.lstEBooks = db.EBooksIndexes.Where(e => e.Title.Contains(search) || e.Author.Contains(search)).Where(e => e.IsActive == true).OrderByDescending(e => e.BooKId).ToList();
                ViewBag.lstSpiritualArticles = db.SpiritualArticles.Where(s => s.Title.Contains(search) || s.Author.Contains(search)).Where(s => s.IsActive == true).OrderByDescending(e => e.SpiritualArticleId).ToList();
                ViewBag.lstMarianBooks = db.MarianBooks.Where(m => m.Title.Contains(search) || m.Author.Contains(search)).Where(m => m.IsActive == true).OrderByDescending(m => m.MarianBookId).ToList();
                ViewBag.lstMarianStudies = db.MarianStudies.Where(m => m.Title.Contains(search) || m.Author.Contains(search)).Where(m => m.IsActive == true).OrderByDescending(m => m.MarianStudyId).ToList();
                ViewBag.lstPrayerRains = (from p in db.Set<PrayerRain>()
                                          orderby p.PrayerRainId descending
                                          where (p.RefPrayerRainId.Contains(search) || p.Title.Contains(search) || p.Verses.Contains(search)) && (p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true)
                                          select new { RefPrayerRainId = p.RefPrayerRainId, CreatedDateAndTime = p.CreatedDateAndTime, PrayerRainId = p.PrayerRainId, Title = p.Title.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = p.Verses.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = p.Prayer.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'") }).ToList()
           .Select(x => new PrayerRain { RefPrayerRainId = x.RefPrayerRainId, PrayerRainId = x.PrayerRainId, Title = System.Text.RegularExpressions.Regex.Replace(x.Title, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = System.Text.RegularExpressions.Regex.Replace(x.Verses, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = System.Text.RegularExpressions.Regex.Replace(x.Prayer, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), CreatedDateAndTime = x.CreatedDateAndTime });
                ViewBag.lstPrayerNotes = db.PrayerNotes.Where(p => p.Title.Contains(search)).Where(p => p.IsActive == true).ToList();
                return document;
            }
        }

        public ActionResult BookRequest()
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
        public ActionResult BookRequest(BookRequest bookRequest)
        {
            if (Session["User"] != null)
            {
                bookRequest.UserId = Convert.ToInt32(Session["UserId"].ToString());
                bookRequest.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                db.BookRequests.Add(bookRequest);
                db.SaveChanges();

                EmailModel obj = new EmailModel();
                var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                obj.ToEmail = "bibleworkshopteam@magnifyelshaddai.com";
                obj.EmailSubject = "Bible Workshop Team - Book Request";
                string Email = "Dear Brother, <br/>Praise the LORD. <br/><br/><br/>I am requesting for the following Book.<br/><br/>" +
                    "<table style='width:40%; margin:auto;'><tbody><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Name</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.Name + "</td></tr><tr><th style='border: 1px solid #ddd; padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Mobile No</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.MobileNo + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Email Address</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.EmailAddress + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Book Title</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.BookTitle + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Book Author</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.BookAuthor + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Book's ISBN</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.BookCallNo + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>About Book</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + bookRequest.AboutBook + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>User Id</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + Session["UserEmail"].ToString() + "</td></tr</tbody></table>";
                obj.EMailBody = Email;
                obj.EmailCC = "";

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
                    string toAddressess = "bibleworkshopteam@magnifyelshaddai.com;sebastianraja@magnifyelshaddai.com;albertsimiyonrajan@magnifyelshaddai.com;arockiasuthan@magnifyelshaddai.com";
                    foreach (var ToAddress in toAddressess.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        message.To.Add(ToAddress);
                    }
                    //message.Bcc.Add(obj.EmailBCC);
                    message.IsBodyHtml = true;
                    message.Subject = obj.EmailSubject;
                    message.Body = obj.EMailBody;
                    smtpClient.Send(message);
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult CommentAndSuggestion()
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
        public ActionResult CommentAndSuggestion(CommentsAndSuggestion commentAndSuggestion)
        {
            if (Session["User"] != null)
            {
                commentAndSuggestion.UserId = Convert.ToInt32(Session["UserId"].ToString());
                commentAndSuggestion.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                db.CommentsAndSuggestions.Add(commentAndSuggestion);
                db.SaveChanges();

                EmailModel obj = new EmailModel();
                var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                obj.ToEmail = "bibleworkshopteam@magnifyelshaddai.com";
                obj.EmailSubject = "Bible Workshop Team - Comments and Suggestions";
                string Email = "Dear Brother, <br/><br/>Praise the LORD. <br/><br/>The following is the my Comments/Suggestions.<br/>" +
                "<table style='font-family: Arial, Helvetica, sans-serif;border-collapse: collapse;width: 40%;margin:auto;'><tbody><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Name</th><td style='border: 1px solid #ddd;padding: 8px 12px;' >" + commentAndSuggestion.Name + "</td></tr><tr><th style='border: 1px solid #ddd; padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Mobile No</th><td style='border: 1px solid #ddd;padding: 8px 12px;' >" + commentAndSuggestion.MobileNo + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Email Address</th><td style='border: 1px solid #ddd;padding: 8px 12px;' >" + commentAndSuggestion.EmailAddress + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>Comments And Suggestions</th><td style='border: 1px solid #ddd;padding: 8px 12px;' >" + commentAndSuggestion.CommentsAndSuggestions + "</td></tr><tr><th style='border: 1px solid #ddd;padding: 8px 12px;border: 1px solid #ddd;text-align: left;background-color: rgb(121, 20, 145, 0.90);color: white;width: 10rem;'>User Id</th><td style='border: 1px solid #ddd;padding: 8px 12px;'>" + Session["UserEmail"].ToString() + "</td></tr></tbody></table>";
                obj.EMailBody = Email;
                obj.EmailCC = "";

                
                    MailMessage mail = new MailMessage();
                    mail.To.Add("benaticgrace@gmail.com");
                    mail.From = new MailAddress("bibleworkshopteam@magnifyelshaddai.com");
                    mail.Subject = "Test Mail";
                    string Body = "Test Mail";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("bibleworkshopteam@magnifyelshaddai.com", "AveMaria@633"); // Enter seders User name and password
                    smtp.EnableSsl = false;
                    smtp.Send(mail);      

                    
                

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
