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
                if (ModelState.IsValid && user.Name != "" && user.Name != null && user.Mobile != "" && user.Mobile != null && user.Email != "" && user.Email != null)
                {
                    string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                    }

                    if (db.Users.Where(x => x.Email == user.Email).Count() == 0)
                    {

                        Random r = new Random();

                        int num = r.Next();

                        var objuser = new User();
                        objuser.Name = user.Name.Trim();
                        objuser.Mobile = user.Mobile.Trim();
                        objuser.Email = user.Email.Trim();
                        objuser.Password = num + "@Jesus";
                        objuser.UserIP = ipAddress;
                        objuser.Location = ipAddress;
                        objuser.UserType = "User";
                        objuser.CreatedDateTime = DateTime.Now;
                        objuser.IsActive = true;
                        objuser.IsNotification = user.IsNotification;
                        db.Users.Add(objuser);
                        db.SaveChanges();

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

                        ViewBag.ErrorMessage = "Your Password has sent to your Email Id!.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "This email address is already registered.So please go to Login!.";
                    }

                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Problem while sending email, Please check details.";
                ViewBag.ErrorMessage1 = ex;
            }

            return View(user);
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
