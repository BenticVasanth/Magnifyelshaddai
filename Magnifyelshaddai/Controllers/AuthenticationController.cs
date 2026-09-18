using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Specialized;
using System.Text;

namespace Magnifyelshaddai.Controllers
{
    public class AuthenticationController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        #region  Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModels model)
        {
            var objuser = db.Users.Where(x => x.IsActive == true & x.Email == model.Email & x.Password == model.Password).FirstOrDefault();
            if (ModelState.IsValid && objuser != null)
            {
                LoggedInDetail loggedInDetailsCreate = new LoggedInDetail();
                loggedInDetailsCreate.LoggedInBy = objuser.Email;
                DateTime currentDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                DateTime currentDate = Convert.ToDateTime(currentDateAndTime.ToString("yyyy-MM-dd"));
                loggedInDetailsCreate.LastLoggedInDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                var loggedInDetailsFromDB = db.LoggedInDetails.Where(x => x.LoggedInBy == objuser.Email).OrderByDescending(x => x.LastLoggedInDateAndTime).FirstOrDefault();

                if (loggedInDetailsFromDB != null)
                {
                    loggedInDetailsFromDB.LastLoggedInDateAndTime = Convert.ToDateTime(loggedInDetailsFromDB.LastLoggedInDateAndTime.Value.ToString("yyyy-MM-dd"));
                    if (loggedInDetailsFromDB.LastLoggedInDateAndTime == currentDate)
                    {
                        LoggedInDetail loggedInDetailsUpdate = new LoggedInDetail();
                        loggedInDetailsUpdate.LoggedInId = loggedInDetailsFromDB.LoggedInId;
                        loggedInDetailsUpdate.LoggedInBy = objuser.Email;
                        loggedInDetailsUpdate.LastLoggedInDateAndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                        loggedInDetailsUpdate.LoggedInCountPerDay = loggedInDetailsFromDB.LoggedInCountPerDay + 1; //Update
                        //db.Entry(loggedInDetailsUpdate).State = EntityState.Modified;
                        db.Entry(loggedInDetailsFromDB).CurrentValues.SetValues(loggedInDetailsUpdate);
                        db.SaveChanges();
                    }
                    else
                    {
                        loggedInDetailsCreate.LoggedInCountPerDay = 1;
                        db.LoggedInDetails.Add(loggedInDetailsCreate);
                        db.SaveChanges();
                    }
                }
                else
                {
                    loggedInDetailsCreate.LoggedInCountPerDay = 1;
                    db.LoggedInDetails.Add(loggedInDetailsCreate);
                    db.SaveChanges();
                }


                Session["User"] = objuser;
                Session["UserName"] = objuser.Name;
                Session["UserEmail"] = objuser.Email;
                Session["UserType"] = objuser.UserType;
                Session["UserId"] = objuser.UserId;
                if (objuser.UserType == "User")
                {
                    return RedirectToAction("Dashboard", "SabbathDay");
                }
                else if (objuser.UserType == "Member" || objuser.UserType == "Librarian")
                {
                    return RedirectToAction("Dashboard", "SabbathDay");
                }
                else if (objuser.UserType == "Subadmin")
                {
                    return RedirectToAction("Dashboard", "SabbathDay");
                }
                else
                {
                    return RedirectToAction("Dashboard", "SabbathDay");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Your Username or Password is incorrect!";
            }

            return View(model);
        }
        #endregion

        #region Registration

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(UserViewModels user)
        {
            try
            {
                // ============================================================
                // VALIDATE reCAPTCHA v3 TOKEN
                // ============================================================

                // Skip reCAPTCHA validation on localhost
                if (!Request.IsLocal)
                {
                    var recaptchaToken = Request.Form["g-recaptcha-response"];

                    if (!ValidateRecaptcha(recaptchaToken))
                    {
                        ViewBag.ErrorMessage =
                            "reCAPTCHA validation failed. Please try again.";

                        return View(user);
                    }
                }

                // ============================================================
                // VALIDATE USER DETAILS
                // ============================================================

                if (ModelState.IsValid &&
                    !string.IsNullOrWhiteSpace(user.Name) &&
                    !string.IsNullOrWhiteSpace(user.Mobile) &&
                    !string.IsNullOrWhiteSpace(user.Email))
                {
                    string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }

                    // ========================================================
                    // CHECK EMAIL ALREADY REGISTERED
                    // ========================================================

                    if (db.Users.Any(x => x.Email == user.Email))
                    {
                        ViewBag.ErrorMessage =
                            "This email address is already registered. So please go to Login!.";

                        return View(user);
                    }

                    // ========================================================
                    // GET EMAIL CONFIGURATION
                    // ========================================================

                    var EmailDB = db.EmailMessages.FirstOrDefault();

                    if (EmailDB == null)
                    {
                        ViewBag.ErrorMessage =
                            "Email configuration not found in database.";

                        return View(user);
                    }

                    if (string.IsNullOrWhiteSpace(EmailDB.EmailID))
                    {
                        ViewBag.ErrorMessage =
                            "Email ID is empty in database.";

                        return View(user);
                    }

                    if (string.IsNullOrWhiteSpace(EmailDB.EmailPassword))
                    {
                        ViewBag.ErrorMessage =
                            "Email password is empty in database.";

                        return View(user);
                    }

                    // ========================================================
                    // GENERATE PASSWORD
                    // ========================================================

                    Random r = new Random();

                    int num = r.Next();

                    string generatedPassword = num + "@Jesus";

                    // ========================================================
                    // CREATE USER
                    // ========================================================

                    var objuser = new User();

                    objuser.Name = user.Name.Trim();
                    objuser.Mobile = user.Mobile.Trim();
                    objuser.Email = user.Email.Trim();
                    objuser.Password = generatedPassword;
                    objuser.UserIP = ipAddress;
                    objuser.Location = ipAddress;
                    objuser.UserType = "User";
                    objuser.CreatedDateTime = DateTime.Now;
                    objuser.IsActive = true;
                    objuser.IsNotification = user.IsNotification;

                    db.Users.Add(objuser);
                    db.SaveChanges();

                    // ========================================================
                    // SEND EMAIL USING SMTP
                    // ========================================================

                    ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls12;

                    string fromEmail = EmailDB.EmailID;
                    string toEmail = objuser.Email;

                    string subject =
                        "Magnify Elshaddai - Confirmation for Registration";

                    string body =
                        "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " +
                        "style=\"background:#f4f2f7;margin:0;padding:12px 8px;\">" +

                            "<tr>" +
                                "<td align=\"center\">" +

                                    // Main Container
                                    "<table width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " +
                                    "style=\"max-width:600px;background:#ffffff;border-radius:8px;" +
                                    "overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);\">" +

                                        // Header
                                        "<tr>" +
                                            "<td align=\"center\" " +
                                            "style=\"background:linear-gradient(138deg,#5a3dbd,#7a1fa2,#a42977);" +
                                            "padding:15px 20px;color:#ffffff;\">" +

                                                "<div style=\"font-size:21px;font-weight:bold;letter-spacing:0.5px;\">" +
                                                    "MAGNIFY EL-SHADDAI" +
                                                "</div>" +

                                                "<div style=\"font-size:14px;margin-top:2px;\">" +
                                                    "விவிலிய பட்டறை குழு" +
                                                "</div>" +

                                            "</td>" +
                                        "</tr>" +

                                        // Content
                                        "<tr>" +
                                            "<td style=\"padding:18px 24px;color:#333333;font-size:15px;line-height:1.5;\">" +

                                                "<p style=\"margin:0 0 8px;\">" +
                                                    "<b>கிறிஸ்து இயேசுவுக்குள் பிரியமான " +
                                                    objuser.Name +
                                                    " அவர்களுக்கு,</b>" +
                                                "</p>" +

                                                "<p style=\"font-size:15px;margin:0 0 10px;color:#7a1fa2;" +
                                                "text-align:center;font-style:italic;line-height:1.5;\">" +
                                                    "நம் தந்தையாம் கடவுளிடமிருந்தும், ஆண்டவராம் இயேசு " +
                                                    "கிறிஸ்துவிடமிருந்தும் அருளும் அமைதியும் உரித்தாகுக!" +
                                                "</p>" +

                                                "<p style=\"margin:0 0 8px;text-align:justify;\">" +
                                                    "<b>MAGNIFY EL-SHADDAI</b> இணையதளத்தில் " +
                                                    "பதிவு செய்ததற்கு நன்றி. " +
                                                    "உங்கள் கணக்கு வெற்றிகரமாக பதிவுசெய்யப்பட்டது." +
                                                "</p>" +

                                                // Account Details
                                                "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " +
                                                "style=\"background:#faf7fc;border:1px solid #e6dced;" +
                                                "border-radius:6px;margin:12px 0;\">" +

                                                    "<tr>" +
                                                        "<td style=\"padding:10px 14px;\">" +

                                                            "<div style=\"color:#7a1fa2;font-size:16px;" +
                                                            "font-weight:bold;margin-bottom:6px;\">" +
                                                                "உங்கள் கணக்கு விவரங்கள்" +
                                                            "</div>" +

                                                            "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">" +

                                                                "<tr>" +
                                                                    "<td style=\"padding:3px 0;color:#666666;" +
                                                                    "width:110px;font-size:15px;\">" +
                                                                        "<b>மின்னஞ்சல் :  </b>" +
                                                                    "</td>" +

                                                                    "<td style=\"padding:3px 0;color:#333333;font-size:15px;\">" +
                                                                        objuser.Email +
                                                                    "</td>" +
                                                                "</tr>" +

                                                                "<tr>" +
                                                                    "<td style=\"padding:3px 0;color:#666666;font-size:15px;\">" +
                                                                        "<b>கடவுச்சொல் :  </b>" +
                                                                    "</td>" +

                                                                    "<td style=\"padding:3px 0;color:#333333;font-size:15px;\">" +
                                                                        generatedPassword +
                                                                    "</td>" +
                                                                "</tr>" +

                                                            "</table>" +

                                                        "</td>" +
                                                    "</tr>" +

                                                "</table>" +

                                                "<p style=\"margin:0 0 10px;color:#555555;text-align:justify;\">" +
                                                    "உங்கள் கணக்கு விவரங்களை பாதுகாப்பாக வைத்துக்கொள்ளவும். " +
                                                    "உங்கள் கடவுச்சொல்லை மற்றவர்களுடன் பகிர வேண்டாம்." +
                                                "</p>" +

                                                // Login Button
                                                "<div style=\"text-align:center;margin:12px 0;\">" +

                                                    "<a href=\"https://www.magnifyelshaddai.com/#elsaddai-signin\" " +
                                                    "style=\"display:inline-block;" +
                                                    "background:#7a1fa2;" +
                                                    "color:#ffffff;" +
                                                    "text-decoration:none;" +
                                                    "padding:8px 22px;" +
                                                    "border-radius:5px;" +
                                                    "font-size:15px;" +
                                                    "font-weight:bold;\">" +

                                                        "Website Login" +

                                                    "</a>" +

                                                "</div>" +

                                                // Bible Verse
                                                "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" " +
                                                "style=\"background:#fdf8ff;border-left:4px solid #7a1fa2;" +
                                                "margin:12px 0;\">" +

                                                    "<tr>" +
                                                        "<td style=\"padding:9px 14px;text-align:center;\">" +

                                                            "<div style=\"font-size:14px;color:#7a1fa2;" +
                                                            "font-weight:bold;margin-bottom:4px;\">" +
                                                                "லூக்கா 1:46–47" +
                                                            "</div>" +

                                                            "<div style=\"font-size:15px;color:#555555;" +
                                                            "font-style:italic;line-height:1.5;\">" +

                                                                "“என் ஆத்துமா கர்த்தரை மகிமைப்படுத்துகிறது.<br>" +
                                                                "என் ஆவி என் இரட்சகராகிய தேவனில் களிகூருகிறது.”" +

                                                            "</div>" +

                                                        "</td>" +
                                                    "</tr>" +

                                                "</table>" +

                                                "<p style=\"margin:10px 0 0;color:#555555;text-align:justify;\">" +
                                                    "எங்களுடன் இணைந்ததற்கு மகிழ்ச்சி. " +
                                                    "உங்கள் ஆன்மீகப் பயணத்தில் வேதவார்த்தையின் மூலம் " +
                                                    "தொடர்ந்து வளர வாழ்த்துகிறோம்." +
                                                "</p>" +

                                                "<p style=\"margin:10px 0 0;font-size:15px;\">" +
                                                    "ஜெபங்களுடனும் நன்றியுடனும்,<br>" +
                                                    "<span style=\"color:#777777;font-size:14px;\">" +
                                                        "விவிலிய பட்டறை குழு" +
                                                    "</span>" +
                                                "</p>" +

                                                "<p style=\"text-align:center;margin:8px 0 0;" +
                                                "color:#7a1fa2;font-weight:bold;font-size:15px;\">" +
                                                    "இயேசுவுக்கே புகழ்! &nbsp; மரியே வாழ்க!" +
                                                "</p>" +

                                            "</td>" +
                                        "</tr>" +

                                        // Footer
                                        "<tr>" +
                                            "<td align=\"center\" " +
                                            "style=\"background:#faf9fb;border-top:1px solid #eee8f1;" +
                                            "padding:10px 20px;font-size:12px;color:#777777;\">" +

                                                "<div style=\"margin-bottom:2px;\">" +
                                                    "© 2026 MAGNIFY EL-SHADDAI" +
                                                "</div>" +

                                                "<a href=\"https://magnifyelshaddai.com/\" " +
                                                "style=\"color:#7a1fa2;text-decoration:none;\">" +
                                                    "www.magnifyelshaddai.com" +
                                                "</a>" +

                                            "</td>" +
                                        "</tr>" +

                                    "</table>" +

                                "</td>" +
                            "</tr>" +

                        "</table>";

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
                            message.From = new MailAddress(fromEmail);

                            // TO
                            message.To.Add(toEmail);

                            // CC
                            message.CC.Add(
                                "bibleworkshopteam@magnifyelshaddai.com"
                            );

                            message.Subject = subject;
                            message.Body = body;
                            message.IsBodyHtml = true;

                            smtpClient.Send(message);
                        }
                    }

                    // ========================================================
                    // SUCCESS
                    // ========================================================

                    ViewBag.Message = "Registration successful! Your password has been sent to your email.";

                    ViewBag.Success = true;
                }
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "SMTP Error: " + ex.ToString()
                );

                ViewBag.ErrorMessage =
                    "Problem while sending email. Please check your email configuration.";

                ViewBag.ErrorMessage1 = ex;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Registration Error: " + ex.ToString()
                );

                ViewBag.ErrorMessage =
                    "Problem while registration. Please check details.";

                ViewBag.ErrorMessage1 = ex;
            }

            if (ViewBag.Success == true)
            {
                ModelState.Clear();
                return View(new UserViewModels());
            }

            return View(user);
        }

        private bool ValidateRecaptcha(string token)
        {
            try
            {
                var secret = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
                if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(token))
                    return false;

                var userIp = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(userIp))
                    userIp = Request.UserHostAddress;

                using (var client = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        { "secret", secret },
                        { "response", token },
                        { "remoteip", userIp }
                    };

                    var response = client.UploadValues(
                        "https://www.google.com/recaptcha/api/siteverify",
                        "POST",
                        data);

                    var responseString = Encoding.UTF8.GetString(response);

                    dynamic jsonData = System.Web.Helpers.Json.Decode(responseString);

                    if (jsonData == null || jsonData.success != true)
                        return false;

                    // ✅ Action check
                    if (jsonData.action == null || jsonData.action.ToString() != "register")
                        return false;

                    // ✅ Hostname check
                    if (jsonData.hostname == null ||
                        jsonData.hostname.ToString() != Request.Url.Host)
                        return false;

                    // ✅ Score check (use decimal literal to match decimal type)
                    decimal score = jsonData.score != null
                        ? Convert.ToDecimal(jsonData.score)
                        : 0m;

                    if (score < 0.7m)
                    {
                        System.Diagnostics.Debug.WriteLine("Low reCAPTCHA score: " + score);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("reCAPTCHA Exception: " + ex.Message);
                return false;
            }
        }

        public static string GetIpAddress()  //Get IP Address
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "--";
        }


        #endregion

        #region Logout
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Authentication");
        }
        #endregion

        #region Forgot Password

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(LoginViewModels model)
        {
            var objuser = db.Users.Where(x => x.IsActive == true & x.Email == model.Email).FirstOrDefault();
            if (objuser != null)
            {
                var primaryUserId = objuser.UserId;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var userUrl = "https://magnifyelshaddai.com/mailtest.php?action=mail&type=userlogin&id=" + primaryUserId;

                var userHttpRequest = (HttpWebRequest)WebRequest.Create(userUrl);

                userHttpRequest.Accept = "application/json";


                var userHttpResponse = (HttpWebResponse)userHttpRequest.GetResponse();
                using (var streamReader = new StreamReader(userHttpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }

                Console.WriteLine(userHttpResponse.StatusCode);

                ViewBag.SuccessMessage = "Your Password has sent to your Email Id!.";
            }
            else
            {
                ViewBag.ErrorMessage = "Please enter the valid Email Id.";
            }

            return View(model);
        }

        #endregion

        #region Registration Informations
        public ActionResult RegistrationInfo()
        {
            //var doc = db.RegistrationMasters.OrderByDescending(x => x.RMID).ToList();
            //ViewBag.LatestDocs = doc;
            var registrationMaster = new RegistrationMasterView
            {
                registrationMaster = db.RegistrationMasters.Where(c => c.BWSID == 3).OrderByDescending(c => c.RMID).ToList()
            };

            ViewBag.RegistrationCount = registrationMaster.registrationMaster.Count();

            return View(registrationMaster);
        }

        public class RegistrationMasterView
        {
            public IEnumerable<RegistrationMaster> registrationMaster { get; set; }
        }
        #endregion

        #region Change Password
        public ActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(PasswordModel model)
        {
            var userId = Session["UserId"].ToString();
            int UserId = Convert.ToInt32(userId);
            var objuser = db.Users.Where(x => x.IsActive == true & x.UserId == UserId).SingleOrDefault();
            if (objuser != null && model.OldPassword != null)
            {
                //Email Send all
                if (model.OldPassword == objuser.Password)
                {
                    if (model.NewPassword != null && model.OldPassword != null)
                    {
                        if (model.OldPassword != model.NewPassword)
                        {
                            if (model.NewPassword == model.ConfirmPassword)
                            {
                                objuser.Password = model.NewPassword;
                                db.Entry(objuser).State = EntityState.Modified;
                                db.SaveChanges();
                                ModelState.Clear();

                                var primaryUserId = objuser.UserId;

                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                var userUrl = "https://magnifyelshaddai.com/mailtest.php?action=mail&type=userlogin&id=" + primaryUserId;

                                var userHttpRequest = (HttpWebRequest)WebRequest.Create(userUrl);

                                userHttpRequest.Accept = "application/json";

                                var userHttpResponse = (HttpWebResponse)userHttpRequest.GetResponse();
                                using (var streamReader = new StreamReader(userHttpResponse.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();
                                }

                                Console.WriteLine(userHttpResponse.StatusCode);
                                ViewBag.SuccessMessage = "Congratulations! Your password has been changed successfully. We have sent your User ID and New Password to your registered email address";

                                return View();
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "The New and Old Passwords cannot be the same.";
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Please enter the correct Current Password.";
                }

            }

            return View(model);
        }
        #endregion


        #region Users Informations
        public ActionResult UserInfo(int page = 1, string search = "")
        {
            if (Session["User"] != null)
            {
                int pageSize = 10;
                int totalRecord = 0;
                if (page < 1) page = 1;
                int skip = (page * pageSize) - pageSize;
                var data = GetUserInfo(search, skip, pageSize, out totalRecord);
                ViewBag.TotalRows = totalRecord;
                ViewBag.search = search;
                ViewBag.UserCount = totalRecord;
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }

        public UserMasterView GetUserInfo(string search, int skip, int pageSize, out int totalRecord)
        {
            using (ElshaddaiDBContext db = new ElshaddaiDBContext())
            {
                var userMaster = new UserMasterView();
                userMaster.userMaster = db.Users.Where(u => u.Name.Contains(search) || u.Email.Contains(search)).Where(u => u.IsActive == true).OrderByDescending(u => u.UserId).ToList();
                totalRecord = userMaster.userMaster.Count();

                if (pageSize > 0)
                {
                    userMaster.userMaster = userMaster.userMaster.Skip(skip).Take(pageSize);
                }
                return userMaster;
            }
        }

        public class UserMasterView
        {
            public IEnumerable<User> userMaster { get; set; }
        }
        #endregion

    }
}
