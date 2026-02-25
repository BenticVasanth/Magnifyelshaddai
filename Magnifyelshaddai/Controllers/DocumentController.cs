using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using context = System.Web.HttpContext;

namespace Magnifyelshaddai.Controllers
{
    public class DocumentsController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static String ErrorlineNo, Errormsg, extype, exurl, hostIp, ErrorLocation, HostAdd;

        // GET: /Document/
        public ActionResult Documents()
        {
            string data;
            if (Session["tabID"] != null)
            {
                data = Session["tabID"].ToString();
                if (data != "")
                    ViewBag.tabID = data;
                else
                    ViewBag.tabID = "lisabbathday";
            }
            else
            {
                data = "lisabbathday";
                ViewBag.tabID = "lisabbathday";
            }

            if (Session["User"] != null)
            {
                var doc = db.Documents.OrderByDescending(x => x.IntId).Take(3).ToList();
                ViewBag.LatestDocs = doc;

                var document = new DocumentView
                {
                    lstSabbathDayDocuments = db.Documents.Where(c => c.Status == true).OrderByDescending(c => c.IntId).ToList(),
                    lstEBooks = db.EBooksIndexes.Where(c => c.IsActive == true).OrderByDescending(c => c.BooKId).ToList(),
                    lstSpiritualArticles = db.SpiritualArticles.Where(c => c.IsActive == true).OrderByDescending(c => c.SpiritualArticleId).ToList(),
                    lstPrayerNotes = db.PrayerNotes.Where(c => c.IsActive == true).OrderByDescending(c => c.PrayerDate).ToList(),
                    lstMarianBooks = db.MarianBooks.Where(c => c.IsActive == true).OrderByDescending(c => c.MarianBookId).ToList(),
                    lstMarianStudies = db.MarianStudies.Where(c => c.IsActive == true).OrderByDescending(c => c.MarianStudyId).ToList(),
                    lstPrayerRains = (from p in db.Set<PrayerRain>()
                                      orderby p.PrayerRainId descending
                                      where p.IsEditable == false && p.IsEmailSent == true && p.IsActive == true && p.IsApproved == true
                                      select new { RefPrayerRainId = p.RefPrayerRainId, CreatedDateAndTime = p.CreatedDateAndTime, PrayerRainId = p.PrayerRainId, Title = p.Title.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = p.Verses.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = p.Prayer.Replace("<[^>]+>|&nbsp;", "").Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'") }).ToList()
           .Select(x => new PrayerRain { RefPrayerRainId = x.RefPrayerRainId, PrayerRainId = x.PrayerRainId, Title = System.Text.RegularExpressions.Regex.Replace(x.Title, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Verses = System.Text.RegularExpressions.Regex.Replace(x.Verses, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), Prayer = System.Text.RegularExpressions.Regex.Replace(x.Prayer, @"<[^>]+>|&nbsp;", String.Empty).Replace("&ldquo;", "\"").Replace("&quot;", "\"").Replace("&rdquo;", "\"").Replace("&#39;", "'"), CreatedDateAndTime = x.CreatedDateAndTime })
                };

                ViewBag.Document = document;
                ViewBag.UserType = Session["UserType"].ToString();

                return View(document);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        [HttpPost]
        public ActionResult DocumentIndex(string txtfilter)
        {
            if (Session["User"] != null)
            {
                Session["txtfilter"] = txtfilter;
                List<Document> document = new List<Document>();
                if (!string.IsNullOrWhiteSpace(txtfilter))
                {

                    document = db.Documents.Where(p => p.Title.Contains(txtfilter)).Where(p => p.Status == true).OrderByDescending(p => p.IntId).ToList();
                }
                else
                {
                    document = db.Documents.Where(p => p.Status == true).OrderByDescending(p => p.IntId).ToList();
                    return PartialView("_DocumentIndex", document);
                }

                return PartialView("_DocumentIndex", document);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult Admin()
        {
            if (Session["User"] != null && Session["UserType"].ToString() == "Admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public ActionResult PrayerRainDownloadFile(int? id)
        {
            if (Session["User"] != null && id != null)
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                //var objPrayerRain = new PrayerRainDownloadDetail();
                //objPrayerRain.PrayerRainId = id;
                //objPrayerRain.DownloadedBy = Session["UserEmail"].ToString();
                //objPrayerRain.DownloadedDateTime = DateTime.Now;
                //objPrayerRain.DownloadeIP = ipAddress;
                ////objDoc.DownloadedLocation = GetIpAddress();
                //objPrayerRain.UserId = Convert.ToInt32(Session["UserId"]);
                //db.PrayerRainDownloadDetails.Add(objPrayerRain);
                //db.SaveChanges();

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
                //string filepath = System.Web.Hosting.HostingEnvironment.MapPath("~/www.magnifyelshaddai.com/ExceptionDetailsFile/");  //Text File Path

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

        [HttpPost]
        public ActionResult Admin(string test)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var url = "https://magnifyelshaddai.com/mailtest.php?action=mail&type=userregister&id=208";
            //var url = "https://magnifyelshaddai.com/mailtest.php?action=mail&type=userlogin&id=12";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Accept = "application/json";


            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            Console.WriteLine(httpResponse.StatusCode);
            return View();
        }

        private IEnumerable<SelectListItem> GetSelectListItems()
        {
            var selectList = new List<SelectListItem>();
            foreach (var element in db.Categories.ToList())
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.CategoryId.ToString(),
                    Text = element.CategoryName
                });
            }
            return selectList;
        }

        private IEnumerable<SelectListItem> GetSelectListItemsForPrayerRain()
        {
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "3",
                Text = "Prayer Rain"
            });
            return selectList;
        }
    }
}
