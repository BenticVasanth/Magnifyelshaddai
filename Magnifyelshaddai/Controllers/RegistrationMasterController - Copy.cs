using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
                    var totalAmnt = "";
                    if (participantType == "Student")
                    {
                        totalAmnt = "Rs.150";
                    }
                    else
                    {
                        totalAmnt = "Rs.500";
                    }

                    //Email Send all

                    //EmailModel obj = new EmailModel();

                    //var EmailDB = db.EmailMessages.SingleOrDefault();
                    //obj.ToEmail = model.Email;
                    //obj.EmailSubject = "Bible Workshop Team";
                    //obj.EMailBody = "Praise the LORD " + objuser.Name + "!" + "<br/>" + "Your Password:" + " " + objuser.Password + "<br/><br/>" + "<b>" + "46. And Mary said, My soul doth magnify the Lord, " + "<br/>" + "47. And my spirit hath rejoiced in God my Saviour. (Luke:1:46-47.)" + "</b>" + "<br/><br/>" + "in Christ," + "<br/>" + "Bible workshop team.";
                    //obj.EmailCC = "";
                    //obj.EmailBCC = "arockiadaniel11@gmail.com";


                    EmailModel objemail = new EmailModel();

                    var EmailDB = db.EmailMessages.SingleOrDefault();
                    objemail.ToEmail = emailID.Trim();
                    objemail.EmailSubject = "Bible Workshop Team - Tirunelveli";
                    objemail.EMailBody = "Praise the LORD " + obj.Name + "!" + "<br/> <br/>" + "Thank you for your Registration to participate in Bible workshop meeting." + "</br>" + "You need to pay in register counter at the time of registration " + totalAmnt + "<br/><br/>" + "<b>" + "13. Enter ye in at the strait gate: for wide  is the gate, and broad  is the way, that leadeth to destruction, and many there be which go in thereat " + "<br/>" + "14. Because strait  is the gate, and narrow  is the way, which leadeth unto life, and few there be that find it. (Matthew:- 7:13-14.)" + "</b>" + "<br/><br/>" + "<b> Contact Number:- Mr.Christopher: +91-9600278407, Mr.Joel: +91-8248469608 </b>" + "<br/><br/>" + "in Christ," + "<br/>" + "Bible workshop team.";
                    objemail.EmailCC = "";
                    objemail.EmailBCC = "joelriraj@gmail.com";
                    //objemail.EmailBCC = "arockiadaniel11@gmail.com";

                    var objuser = db.Users.Where(x => x.IsActive == true & x.Email.Trim() == emailID.Trim()).SingleOrDefault();
                    if (objuser == null)
                    {
                        //Email Send 
                        User user = new User();

                        user.Email = emailID.Trim();
                        user.Name = name.Trim();
                        user.Mobile = mobileNo;
                        user.Password = name.Replace(" ","") + "@2024";
                        user.UserIP = ipAddress;
                        user.Location = ipAddress;
                        user.UserType = "User";
                        user.IsActive = true;
                        user.CreatedDateTime = DateTime.Now;
                        db.Users.Add(user);
                        db.SaveChanges();

                        objemail.EMailBody = "Praise the LORD " + obj.Name + "!" + "<br/>" + "Thank you for your Registration to participate in Bible workshop meeting and Magnify Elshaddai application. Your Password is " + user.Password + "." + "</br>" + "You need to pay in register counter at the time of  registration " + totalAmnt + "<br/><br/>" + "<b>" + "46. And Mary said, My soul doth magnify the Lord, " + "<br/>" + "47. And my spirit hath rejoiced in God my Saviour. (Luke:1:46-47.)" + "</b>" + "<br/><br/>" + "in Christ," + "<br/>" + "Bible workshop team.";
                    }

                    try
                    {
                        using (SmtpClient smtpClient = new SmtpClient())
                        {
                            smtpClient.Host = "relay-hosting.secureserver.net";
                            //smtpClient.Host = "smtp.gmail.com";
                            smtpClient.Port = 25;
                            smtpClient.EnableSsl = false;
                            MailMessage message = new MailMessage();
                            MailAddress fromAddress = new MailAddress(EmailDB.EmailID);
                            MailAddress toAddress = new MailAddress(objemail.ToEmail);
                            smtpClient.Credentials = new System.Net.NetworkCredential(EmailDB.EmailID, EmailDB.EmailPassword);
                            message.From = fromAddress;
                            message.To.Add(toAddress);
                            message.Bcc.Add(objemail.EmailBCC);
                            message.IsBodyHtml = true;
                            message.Subject = objemail.EmailSubject;
                            message.Body = objemail.EMailBody;
                            smtpClient.Send(message);
                            ViewBag.ErrorMessage = "Your Password has sent to your Email Id!.";
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.ErrorMessage = "Problem while sending email, Please check details.";
                        ViewBag.ErrorMessage1 = ex;
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
