using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magnifyelshaddai.Models.EDMXModel;
using Magnifyelshaddai.Models;
using System.Net.Mail;
using System.Web.Helpers;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

using System.Diagnostics;
using System.Text;
using SelectPdf;
using System.Text.RegularExpressions;
using System.Net.Mime;
using System.IO;
using context = System.Web.HttpContext;


namespace Magnifyelshaddai.Controllers
{
    public class PrayerRainController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static String ErrorlineNo, Errormsg, extype, exurl, hostIp, ErrorLocation, HostAdd;

        public static void SendErrorToText(Exception ex)
        {
            var line = Environment.NewLine + Environment.NewLine;

            ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
            Errormsg = ex.GetType().Name.ToString();
            extype = ex.GetType().ToString();
            exurl = context.Current.Request.Url.ToString();
            ErrorLocation = ex.Message.ToString();

            try
            {
                string filepath = context.Current.Server.MapPath("~/ExceptionDetailsFile/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!System.IO.File.Exists(filepath))
                {
                    System.IO.File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = System.IO.File.AppendText(filepath))
                {
                    string error = "Log Written Date:" + " " + DateTime.Now.ToString() + line + "Error Line No :" + " " + ErrorlineNo + line + "Error Message:" + " " + Errormsg + line + "Exception Type:" + " " + extype + line + "Error Location :" + " " + ErrorLocation + line + " Error Page Url:" + " " + exurl + line + "User Host IP:" + " " + hostIp + line;
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("-------------------------------------------------------------------------------------");
                    sw.WriteLine(line);
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------*End*------------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();

                }
            }
            catch (Exception e)
            {
                e.ToString();

            }
        }

        public ActionResult Index()
        {
            int createdBy = 0;
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Member")
                {
                    List<PrayerRain> lstPrayerRain = new List<PrayerRain>();
                    lstPrayerRain = db.PrayerRains.Where(p => p.IsEditable == false && p.IsEmailSent == false && p.IsActive == true && p.IsApproved == false).OrderByDescending(p => p.PrayerRainId).ToList();

                    foreach (PrayerRain prayerRain in lstPrayerRain)
                    {
                        var Users = db.Users.Where(u => u.UserId == prayerRain.CreatedBy).FirstOrDefault();
                        lstPrayerRain.Where(p => p.PrayerRainId == prayerRain.PrayerRainId).FirstOrDefault().PrayerRainCreatedBy = Users.Name;

                        var UsersUploadedBy = db.Users.Where(u => u.UserId == prayerRain.UploadedBy).FirstOrDefault();
                        if (UsersUploadedBy != null)
                            lstPrayerRain.Where(p => p.PrayerRainId == prayerRain.PrayerRainId).FirstOrDefault().PrayerRainUploadedBy = UsersUploadedBy.Name;
                    }
                    ViewBag.AdminProvisionToBeApprovedPrayerRainForMember = lstPrayerRain;

                    //ViewBag.ApprovedPrayerRainForMember = PrayerRains.Where(p => p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true).ToList();
                }

                if (Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    List<PrayerRain> lstPrayerRain = new List<PrayerRain>();
                    lstPrayerRain = db.PrayerRains.Where(p => p.IsEditable == false && p.IsEmailSent == false && p.IsActive == true && p.IsApproved == false && p.PrayerRainFilePath != null).OrderByDescending(p => p.PrayerRainId).ToList();

                    foreach (PrayerRain prayerRain in lstPrayerRain)
                    {
                        var Users = db.Users.Where(u => u.UserId == prayerRain.CreatedBy).FirstOrDefault();
                        lstPrayerRain.Where(p => p.PrayerRainId == prayerRain.PrayerRainId).FirstOrDefault().PrayerRainCreatedBy = Users.Name;

                        var UsersUploadedBy = db.Users.Where(u => u.UserId == prayerRain.UploadedBy).FirstOrDefault();
                        if (UsersUploadedBy != null)
                            lstPrayerRain.Where(p => p.PrayerRainId == prayerRain.PrayerRainId).FirstOrDefault().PrayerRainUploadedBy = UsersUploadedBy.Name;
                    }
                    ViewBag.AdminProvisionToBeApprovedPrayerRainForAdmin = lstPrayerRain;

                    //ViewBag.ApprovedPrayerRainForMember = PrayerRains.Where(p => p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true).ToList();
                }

                createdBy = Convert.ToInt32(Session["UserId"]);
                ViewBag.NotEditableForUser = db.PrayerRains.Where(p => p.CreatedBy == createdBy && p.IsEditable == false && p.IsActive == true).OrderByDescending(p => p.PrayerRainId).ToList();
                ViewBag.EditableForUser = db.PrayerRains.Where(p => p.CreatedBy == createdBy && p.IsEditable == true && p.IsActive == true).OrderByDescending(p => p.PrayerRainId).ToList();

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        //For Final result to All
        public ActionResult PrayerRain(int page = 1, string search = "")
        {
            List<PrayerRainVM> lstPrayerRainDownloadDetail = new List<PrayerRainVM>();

            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetPrayerRainForFinalResult(search, skip, pageSize, out totalRecord);
                foreach (var prayerRain in data.lstPrayerRains)
                {
                    List<string> lstPrayerRainDownloadedUsers = db.PrayerRainDownloadDetails.Where(pr => pr.PrayerRainId == prayerRain.PrayerRainId).Select(pr => pr.DownloadedBy).Distinct().ToList();
                    List<PrayerRainDownloadDetail> lstPrayerRainDownloadDetails = new List<PrayerRainDownloadDetail>();
                    foreach (string user in lstPrayerRainDownloadedUsers)
                    {
                        lstPrayerRainDownloadDetails.Add(new PrayerRainDownloadDetail { DownloadedBy = user });
                    }
                    prayerRain.totalDownloadedCount = lstPrayerRainDownloadedUsers.Count();
                    lstPrayerRainDownloadDetail.Add(new PrayerRainVM { prayerRain = prayerRain, lstPrayerRainDownloadDetails = lstPrayerRainDownloadDetails });
                }
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                return View(lstPrayerRainDownloadDetail);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public DocumentView GetPrayerRainForFinalResult(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var document = new DocumentView();
                document.lstPrayerRains = (from p in db.Set<PrayerRain>()
                                           orderby p.PrayerRainId descending
                                           where (p.RefPrayerRainId.Contains(search) || p.Title.Contains(search) || p.Verses.Contains(search)) && (p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true)
                                           select new { RefPrayerRainId = p.RefPrayerRainId, CreatedDateAndTime = p.CreatedDateAndTime, PrayerRainId = p.PrayerRainId, Title = p.Title.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = p.Verses.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = p.Prayer.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'") }).ToList()
           .Select(x => new PrayerRain { RefPrayerRainId = x.RefPrayerRainId, PrayerRainId = x.PrayerRainId, Title = System.Text.RegularExpressions.Regex.Replace(x.Title, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = System.Text.RegularExpressions.Regex.Replace(x.Verses, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = System.Text.RegularExpressions.Regex.Replace(x.Prayer, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), CreatedDateAndTime = x.CreatedDateAndTime });

                totalRecord = document.lstPrayerRains.Count();

                if (pageSize > 0)
                {
                    document.lstPrayerRains = document.lstPrayerRains.Skip(skip).Take(pageSize);
                }
                return document;
            }
        }

        [NonAction]
        public string RemoveHTML(string strHTML)
        {
            return Regex.Replace(strHTML, "<(.|\n)*?>", "@");
        }

        public ActionResult Details(int id = 0)
        {
            if (Session["User"] != null)
            {
                PrayerRain prayerrain = db.PrayerRains.Find(id);
                if (prayerrain == null)
                {
                    return HttpNotFound();
                }
                return View(prayerrain);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult Create()
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
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(PrayerRainViewModel vmPrayerRain)
        {
            if (Session["User"] != null)
            {
                if (ModelState.IsValid)
                {
                    var prayerRain = new PrayerRain();
                    prayerRain.Title = vmPrayerRain.Title;
                    prayerRain.Verses = vmPrayerRain.Verses;
                    prayerRain.Prayer = vmPrayerRain.Prayer;

                    prayerRain.CreatedBy = Convert.ToInt32(Session["UserId"]);
                    prayerRain.CreatedDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    prayerRain.EditEndDateAndTime = Convert.ToDateTime(prayerRain.CreatedDateAndTime).AddMinutes(60);

                    if (Session["UserType"].ToString() == "Member")
                        prayerRain.IsEditable = false;
                    else
                        prayerRain.IsEditable = true;

                    prayerRain.IsEmailSent = false;
                    prayerRain.IsActive = true;
                    prayerRain.IsApproved = false;

                    int prayerRainIDToBeCreated;
                    if (db.PrayerRains.Count() > 0)
                    {
                        prayerRainIDToBeCreated = db.PrayerRains.Max(p => p.PrayerRainId);
                    }
                    else
                    {
                        prayerRainIDToBeCreated = 0;
                    }

                    prayerRainIDToBeCreated = prayerRainIDToBeCreated + 1;
                    string currentDateAndTime = ((DateTime)prayerRain.CreatedDateAndTime).ToString("ddMMMyyyyHHmm");

                    string prayerRainID = string.Empty;
                    if (prayerRainIDToBeCreated <= 10000)
                        prayerRainID = "PR" + prayerRainIDToBeCreated.ToString("00000");
                    else
                        prayerRainID = "PR" + prayerRainIDToBeCreated.ToString("00000");

                    prayerRain.RefPrayerRainId = prayerRainID;
                    prayerRain.FileName = prayerRainID + "_" + currentDateAndTime + ".pdf";

                    db.PrayerRains.Add(prayerRain);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                return View(vmPrayerRain);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult Edit(int id = 0)
        {
            if (Session["User"] != null)
            {
                PrayerRain prayerRain = db.PrayerRains.Find(id);
                if (prayerRain == null)
                {
                    return HttpNotFound();
                }

                PrayerRainViewModel vmPrayerRain = new PrayerRainViewModel();
                vmPrayerRain.PrayerRainId = prayerRain.PrayerRainId;
                vmPrayerRain.Title = prayerRain.Title;
                vmPrayerRain.Verses = prayerRain.Verses;
                vmPrayerRain.Prayer = prayerRain.Prayer;
                vmPrayerRain.CreatedBy = prayerRain.CreatedBy;
                vmPrayerRain.CreatedDateAndTime = prayerRain.CreatedDateAndTime;
                vmPrayerRain.EditEndDateAndTime = prayerRain.EditEndDateAndTime;
                vmPrayerRain.IsEditable = prayerRain.IsEditable;
                vmPrayerRain.IsEmailSent = prayerRain.IsEmailSent;
                vmPrayerRain.IsActive = prayerRain.IsActive;
                vmPrayerRain.IsApproved = prayerRain.IsApproved;
                vmPrayerRain.ApprovedBy = prayerRain.ApprovedBy;
                vmPrayerRain.ApprovedDateTime = prayerRain.ApprovedDateTime;
                vmPrayerRain.FileName = prayerRain.FileName;
                vmPrayerRain.RefPrayerRainId = prayerRain.RefPrayerRainId;

                return View(vmPrayerRain);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(PrayerRainViewModel vmPrayerRain)
        {
            if (Session["User"] != null)
            {
                if (ModelState.IsValid)
                {
                    PrayerRain prayerRain = new PrayerRain();
                    prayerRain.PrayerRainId = vmPrayerRain.PrayerRainId;
                    prayerRain.Title = vmPrayerRain.Title;
                    prayerRain.Verses = vmPrayerRain.Verses;
                    prayerRain.Prayer = vmPrayerRain.Prayer;
                    prayerRain.CreatedBy = vmPrayerRain.CreatedBy;
                    prayerRain.CreatedDateAndTime = vmPrayerRain.CreatedDateAndTime;
                    prayerRain.EditEndDateAndTime = vmPrayerRain.EditEndDateAndTime;
                    prayerRain.IsEditable = vmPrayerRain.IsEditable;
                    prayerRain.IsEmailSent = vmPrayerRain.IsEmailSent;
                    prayerRain.IsActive = vmPrayerRain.IsActive;
                    prayerRain.IsApproved = vmPrayerRain.IsApproved;
                    prayerRain.ApprovedBy = vmPrayerRain.ApprovedBy;
                    prayerRain.ApprovedDateTime = vmPrayerRain.ApprovedDateTime;
                    prayerRain.FileName = vmPrayerRain.FileName;
                    prayerRain.RefPrayerRainId = vmPrayerRain.RefPrayerRainId;

                    db.Entry(prayerRain).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(vmPrayerRain);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["User"] != null)
            {
                PrayerRain prayerrain = db.PrayerRains.Find(id);
                prayerrain.IsEditable = false;

                if (Session["UserType"].ToString() == "Member" || Session["UserType"].ToString() == "Admin" || Session["UserType"].ToString() == "Subadmin")
                {
                    prayerrain.PrayerRainFilePath = null;
                    if (!string.IsNullOrEmpty(prayerrain.PrayerRainFilePath))
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/"), prayerrain.PrayerRainFilePath));
                }
                else if (Session["UserType"].ToString() == "User")
                    prayerrain.IsActive = false;
                //db.PrayerRains.Remove(prayerrain);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult DownloadPrayerRain(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                var objPrayerRain = new PrayerRainDownloadDetail();
                objPrayerRain.PrayerRainId = id;
                objPrayerRain.DownloadedBy = Session["UserEmail"].ToString();
                objPrayerRain.DownloadedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objPrayerRain.DownloadeIP = ipAddress;
                //objDoc.DownloadedLocation = GetIpAddress();
                objPrayerRain.UserId = Convert.ToInt32(Session["UserId"]);
                db.PrayerRainDownloadDetails.Add(objPrayerRain);
                db.SaveChanges();

                var objPrayerRainDocument = db.PrayerRains.Where(x => x.IsActive == true & x.PrayerRainId == id).SingleOrDefault();
                if (!string.IsNullOrEmpty(objPrayerRainDocument.PrayerRainFilePath))
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + objPrayerRainDocument.PrayerRainFilePath;
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    string fileName = System.IO.Path.GetFileName(path);
                    return File(fileBytes, "application/unknown", fileName);
                }
                else
                {
                    return RedirectToAction("Index", "PrayerRain");
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        [HttpPost]
        public ActionResult Approve(int id)
        {
            string imgFile = Server.MapPath("~/Images/Prayer_Rain_Logo_Email.jpg");
            LinkedResource Img = new LinkedResource(imgFile, MediaTypeNames.Image.Jpeg);
            Img.ContentId = "PrayerRainLogo";
            try
            {
                if (Session["User"] != null)
                {
                    var prayerRain = db.PrayerRains.Where(p => p.PrayerRainId == id).FirstOrDefault();
                    var bibleVersesPostedBy = db.Users.Where(u => u.UserId == prayerRain.CreatedBy).FirstOrDefault();

                    EmailModel obj = new EmailModel();
                    var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                    obj.ToEmail = bibleVersesPostedBy.Email;
                    obj.EmailSubject = "Magnify Elshaddai - Prayer Rain";
                    obj.EMailBody = "Dear Brother in Christ, <br/> <b>Praise the LORD</b> <br/> Based on your request Prayer Rain document has been uploaded in our site(www.magnifyelshaddai.com) for the Greater Glory of GOD." + "<br/><br/><span style='font-family:arial;font-size:15px;'><b>For Reference : </b></span>" + prayerRain.RefPrayerRainId + "<br/>Bible Verses Posted By : " + bibleVersesPostedBy.Email + "</br><br/>Created Date and Time : " + prayerRain.CreatedDateAndTime + "<br/><b>Ave Maria</b><br/><br/>Thanks,<br/>" + "Bible Workshop Team";

                    //obj.EmailBCC = "bibleworkshopteam@magnifyelshaddai.com";

                    //Live Email Configuration - Starts
                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = "relay-hosting.secureserver.net";
                        smtpClient.Port = 25;
                        smtpClient.EnableSsl = false;
                        MailMessage message = new MailMessage();
                        MailAddress fromAddress = new MailAddress("prayerrainisaiah551011@gmail.com");
                        MailAddress toAddress = new MailAddress(obj.ToEmail);
                        smtpClient.Credentials = new System.Net.NetworkCredential("prayerrainisaiah551011@gmail.com", "AveMariaLuke@146");
                        message.From = fromAddress;
                        message.To.Add(toAddress);

                        message.CC.Add("arockiasuthan@magnifyelshaddai.com");

                        //string ccAddressess = "arockiajs93@gmail.com";
                        //foreach (var CCAddress in ccAddressess.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        //{
                        //    message.CC.Add(CCAddress);
                        //}

                        //message.Bcc.Add(obj.EmailBCC);
                        message.IsBodyHtml = true;
                        message.Subject = obj.EmailSubject;
                        message.Body = obj.EMailBody;
                        smtpClient.Send(message);
                        //ViewBag.SuccessMessage = "Your Password has sent to your Email Id!.";
                    }
                    //Live Email Configuration - Ends

                    //        //Local Email Configuration - Starts
                    //        //MailMessage message = new MailMessage();
                    //        //SmtpClient smtp = new SmtpClient();
                    //        //message.From = new MailAddress(EmailDB.EmailID);
                    //        //message.To.Add(new MailAddress(EmailDB.EmailID));
                    //        //message.CC.Add(new MailAddress(obj.EmailCC));
                    //        //message.Bcc.Add(new MailAddress(obj.EmailBCC));
                    //        //message.Subject = obj.EmailSubject;
                    //        //message.IsBodyHtml = true;
                    //        //message.Body = obj.EMailBody;
                    //        //smtp.Port = 25;
                    //        //smtp.Host = "smtp.gmail.com";
                    //        //smtp.EnableSsl = true;
                    //        //smtp.UseDefaultCredentials = false;
                    //        //smtp.Credentials = new NetworkCredential(EmailDB.EmailID, EmailDB.EmailPassword);
                    //        //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //        //smtp.Send(message);
                    //        //Local Email Configuration - Ends

                    db.Entry(prayerRain).State = EntityState.Modified;
                    prayerRain.IsEmailSent = true;
                    prayerRain.ApprovedBy = Convert.ToInt32(Session["UserId"]);
                    prayerRain.IsApproved = true;

                    prayerRain.ApprovedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    string currentDateAndTime = ((DateTime)prayerRain.CreatedDateAndTime).ToString("ddMMMyyyyHHmm");

                    string prayerRainID = string.Empty;
                    if (prayerRain.PrayerRainId <= 10000)
                        prayerRainID = "PR" + prayerRain.PrayerRainId.ToString("00000");
                    else
                        prayerRainID = "PR" + prayerRain.PrayerRainId.ToString("00000");

                    prayerRain.PrayerRainFilePath = "Documents/PrayerRainDocuments/" + prayerRainID + "_" + currentDateAndTime + ".pdf";
                    prayerRain.RefPrayerRainId = prayerRainID;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Authentication");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Problem while sending email, Please check details.";
                ViewBag.ErrorMessage1 = ex;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void UpdateDatabaseFrequently()
        {
            DateTime currentDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            List<PrayerRain> lstPrayerRain = new List<PrayerRain>();
            lstPrayerRain = db.PrayerRains.Where(p => p.IsEditable == true && p.IsEmailSent == false && p.IsActive == true && p.IsApproved == false && currentDateAndTime > p.EditEndDateAndTime).ToList();

            foreach (PrayerRain prayerRain in lstPrayerRain)
            {
                prayerRain.IsEditable = false;
                db.Entry(prayerRain).State = EntityState.Modified;
                db.SaveChanges();

                var bibleVersesPostedBy = db.Users.Where(u => u.UserId == prayerRain.CreatedBy).FirstOrDefault();

                EmailModel obj = new EmailModel();
                var EmailDB = db.EmailMessages.Where(x => x.EID == 1).SingleOrDefault();
                obj.ToEmail = "prayerrainisaiah551011@gmail.com";
                obj.EmailSubject = "Magnify Elshaddai - Prayer Rain";
                obj.EMailBody = "Dear Bible Workshop Team, <br/> <b>Praise the LORD</b> <br/> The following is/are the <b>BIBLE VERSE(S)</b> that blessed me(To Praise GOD/For Confession). So, I am sharing the same with you For the Greater Glory of GOD." + "<br/><br/><span style='font-family:arial;font-size:15px;'><b>For Reference : </b></span>" + prayerRain.RefPrayerRainId + "<br/><br/><span style='font-family:arial;font-size:15px;'><b>Title : </b></span>" + prayerRain.Title + "<br/><span style='font-family:arial;font-size:15px;'><b>Verses : </b></span>" + prayerRain.Verses + "<br/><span style='font-family:arial;font-size:15px;'><b>Prayer : </b></span>" + prayerRain.Prayer + "<br/>Bible Verses Posted By : " + bibleVersesPostedBy.Email + "</br><br/>Created Date and Time : " + prayerRain.CreatedDateAndTime + "<br/><b>Ave Maria</b><br/><br/>Thanks,<br/>" + bibleVersesPostedBy.Name;
                obj.EmailCC = bibleVersesPostedBy.Email;
                //obj.EmailBCC = "bibleworkshopteam@magnifyelshaddai.com";

                //Live Email Configuration - Starts
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = "relay-hosting.secureserver.net";
                    smtpClient.Port = 25;
                    smtpClient.EnableSsl = false;
                    MailMessage message = new MailMessage();
                    MailAddress fromAddress = new MailAddress("prayerrainisaiah551011@gmail.com");
                    MailAddress toAddress = new MailAddress(obj.ToEmail);
                    smtpClient.Credentials = new System.Net.NetworkCredential("prayerrainisaiah551011@gmail.com", "AveMariaLuke@146");
                    message.From = fromAddress;
                    string toAddressess = "sebastianraja@magnifyelshaddai.com;xaviersarc777@gmail.com;asra.ps@gmail.com;devarajsundaram18@gmail.com;joelriraj@gmail.com;michaelraj1963@gmail.com;chrischemo@gmail.com;petersloyola@gmail.com;mathewvinoth@magnifyelshaddai.com;arockiasuthan@magnifyelshaddai.com";
                    foreach (var ToAddress in toAddressess.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        message.To.Add(ToAddress);
                    }
                    message.CC.Add(obj.EmailCC);
                    //message.Bcc.Add(obj.EmailBCC);
                    message.IsBodyHtml = true;
                    message.Subject = obj.EmailSubject;
                    message.Body = obj.EMailBody;
                    smtpClient.Send(message);
                }
                //Live Email Configuration - Ends

                //    //Local Email Configuration - Starts
                //    //MailMessage message = new MailMessage();
                //    //SmtpClient smtp = new SmtpClient();
                //    //message.From = new MailAddress(EmailDB.EmailID);
                //    //message.To.Add(new MailAddress(EmailDB.EmailID));
                //    //message.CC.Add(new MailAddress(obj.EmailCC));
                //    //message.Bcc.Add(new MailAddress(obj.EmailBCC));
                //    //message.Subject = obj.EmailSubject;
                //    //message.IsBodyHtml = true;
                //    //message.Body = obj.EMailBody;
                //    //smtp.Port = 25;
                //    //smtp.Host = "smtp.gmail.com";
                //    //smtp.EnableSsl = true;
                //    //smtp.UseDefaultCredentials = false;
                //    //smtp.Credentials = new NetworkCredential(EmailDB.EmailID, EmailDB.EmailPassword);
                //    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    //smtp.Send(message);
                //    //Local Email Configuration - Ends
            }
        }

        public ActionResult UploadPrayerRain()
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
        public ActionResult UploadPrayerRain(DocumentViewModels Doc, HttpPostedFileBase file)
        {
            if (Session["User"] != null)
            {
                if (Session["UserType"].ToString() == "Member" || Session["UserType"].ToString() == "Admin")
                {
                    int prayerRainID = Convert.ToInt32(Doc.DocumentID);
                    var prayerRain = db.PrayerRains.Where(p => p.PrayerRainId == prayerRainID).FirstOrDefault();
                    if (prayerRain.PrayerRainFilePath == null)
                    {
                        try
                        {

                            string PrayerRainFileName = Path.GetFileName(file.FileName);
                            string PrayerRainUploadPath = Path.Combine(Server.MapPath("../Documents/PrayerRainDocuments"), PrayerRainFileName);
                            file.SaveAs(PrayerRainUploadPath);

                            db.Entry(prayerRain).State = EntityState.Modified;
                            prayerRain.PrayerRainFilePath = "Documents/PrayerRainDocuments/" + PrayerRainFileName;
                            prayerRain.UploadedBy = Convert.ToInt32(Session["UserId"]);
                            db.SaveChanges();
                            ModelState.Remove("DocUpload");
                            ViewBag.Message = "File uploaded successfully.";
                        }
                        catch (Exception ex)
                        {
                            SendErrorToText(ex);
                        }
                    }
                    else
                    {
                        if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/"), prayerRain.PrayerRainFilePath)))
                        {
                            ModelState.Remove("DocUpload");
                            ViewBag.Message = "This Document is already uploaded.";
                        }
                        else
                        {
                            string PrayerRainFileName = Path.GetFileName(file.FileName);
                            string PrayerRainUploadPath = Path.Combine(Server.MapPath("../Documents/PrayerRainDocuments"), PrayerRainFileName);
                            file.SaveAs(PrayerRainUploadPath);

                            db.Entry(prayerRain).State = EntityState.Modified;
                            prayerRain.PrayerRainFilePath = "Documents/PrayerRainDocuments/" + PrayerRainFileName;
                            prayerRain.UploadedBy = Convert.ToInt32(Session["UserId"]);
                            db.SaveChanges();
                            ModelState.Remove("DocUpload");
                            ViewBag.Message = "File uploaded successfully.";
                        }
                    }
                    return View();
                }
                else
                {
                    ViewBag.Message = "Please login admin user!!";
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}