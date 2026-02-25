using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class RegistrationMasterController : Controller
    {
        ElshaddaiDBContext db = new ElshaddaiDBContext();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDetails(string emailId)
        {
            var dtLst = db.Users.Where(m => m.Email == emailId).Select(m => m).ToList();
            var output = dtLst;
            return Json(output, JsonRequestBehavior.AllowGet); 
        }

        [HttpPost]
        public JsonResult Index(string emailID, string name, string gender, int age, string address, string mobileNo, int qualification, string participantType, bool needOfAccommodation)
        {

            var messages = "";
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }

                //int BWSID = db.BibleWorkShops.Where(x=>x.IsActive==true).OrderByDescending(x=>x.BWSID).LastOrDefault().BWSID;
                int BWSID = 3;
                if (db.RegistrationMasters.Where(x => x.EmailId.Trim() == emailID.Trim() && x.BWSID == BWSID).Count() == 0)
                {
                    var obj = new RegistrationMaster();
                    obj.BWSID = BWSID;
                    obj.EmailId = emailID.Trim();
                    obj.Name = name.Trim();
                    obj.Gender = gender.Trim();
                    obj.Age = age;
                    obj.Address = address.Trim();
                    obj.MobileNo = mobileNo.Trim();
                    obj.Qualification = qualification;
                    obj.ParticipantType = participantType.Trim();
                    obj.NeedOfAccommodation = needOfAccommodation;
                    obj.CreatedDateTime = DateTime.Now;
                    db.RegistrationMasters.Add(obj);
                    db.SaveChanges();
                    var primaryKeyValue = obj.RMID;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var url = "https://magnifyelshaddai.com/mailtest.php?action=mail&type=userregister&id=" + primaryKeyValue;

                    var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                    httpRequest.Accept = "application/json";


                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }

                    Console.WriteLine(httpResponse.StatusCode);

                    var objuser = db.Users.Where(x => x.IsActive == true & x.Email.Trim() == emailID.Trim()).SingleOrDefault();
                    if (objuser == null)
                    {
                        //Email Send 
                        User user = new User();

                        user.Email = emailID.Trim();
                        user.Name = name.Trim();
                        user.Mobile = mobileNo;
                        user.Password = name.Replace(" ", "") + "@2024";
                        user.UserIP = ipAddress;
                        user.Location = ipAddress;
                        user.UserType = "User";
                        user.IsActive = true;
                        user.CreatedDateTime = DateTime.Now;
                        db.Users.Add(user);
                        db.SaveChanges();

                        var primaryUserId = user.UserId;

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
                    }
                        messages = "Success";

                        //return RedirectToAction("Index", "Home");
                        return Json(messages, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        messages = "Fail";
                    }

                return Json(messages, JsonRequestBehavior.AllowGet); 
        }
    }
}
