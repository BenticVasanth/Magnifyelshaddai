using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class TestController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendTestMail()
        {
            try
            {
                var EmailDB = db.EmailMessages.FirstOrDefault();

                if (EmailDB == null)
                {
                    ViewBag.Message = "Email configuration not found in database.";
                    ViewBag.Success = false;

                    return View("Index");
                }


                if (string.IsNullOrWhiteSpace(EmailDB.EmailID))
                {
                    ViewBag.Message = "Email ID is empty in database.";
                    ViewBag.Success = false;

                    return View("Index");
                }

                if (string.IsNullOrWhiteSpace(EmailDB.EmailPassword))
                {
                    ViewBag.Message = "Email password is empty in database.";
                    ViewBag.Success = false;

                    return View("Index");
                }

                // ============================================================
                // EMAIL DETAILS
                // ============================================================

                string fromEmail = EmailDB.EmailID;

                // ============================================================
                // SMTP
                // ============================================================

                System.Net.ServicePointManager.SecurityProtocol =
    System.Net.SecurityProtocolType.Tls12;

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;

                    smtpClient.Credentials = new NetworkCredential(
                        EmailDB.EmailID,
                        EmailDB.EmailPassword
                    );

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress(EmailDB.EmailID);
                        message.To.Add("benatic98@gmail.com");
                        //message.CC.Add("bibleworkshopteam@magnifyelshaddai.com");
                        message.Subject = "Magnify Elshaddai - Test Mail";

                        message.Body =
                            "<p>Praise the LORD Brother,</p>" +
                            "<p>This is a test email from Magnify Elshaddai.</p>" +
                            "<p>If you received this email, SMTP is working correctly.</p>";

                        message.IsBodyHtml = true;

                        smtpClient.Send(message);
                    }
                }

                // ============================================================
                // SUCCESS
                // ============================================================

                ViewBag.Message = "Email sent successfully!";
                ViewBag.Success = true;
            }
            catch (SmtpException ex)
            {
                ViewBag.Message = "<pre>" + Server.HtmlEncode(ex.ToString()) + "</pre>";

                ViewBag.Success = false;
            }
            catch (Exception ex)
            {
                ViewBag.Message =
                    "Error: " + ex.Message;

                ViewBag.Success = false;
            }

            return View("Index");
        }
    }
}