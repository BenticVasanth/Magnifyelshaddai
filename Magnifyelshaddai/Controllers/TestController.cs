using Magnifyelshaddai.Models;
using Magnifyelshaddai.Models.EDMXModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class TestController : Controller
    {
        private ElshaddaiDBContext db = new ElshaddaiDBContext();
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestAdmin()
        {
            if (Session["User"] != null && Session["UserType"].ToString() == "Admin")
            {
                var model = new DocumentViewModels();
                model.CategoryList = GetSelectListItems();
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult TestAdmin(HttpPostedFileBase file, DocumentViewModels Doc)
        {
            if (Session["User"] != null)
            {
                var model = new DocumentViewModels();
                model.CategoryList = GetSelectListItems();
                model.CategoryId = Doc.CategoryId;

                try
                {
                    if (file.ContentLength > 0)
                    {
                        if (Session["UserType"].ToString() == "Admin")
                        {
                            string _FileName = Path.GetFileName(file.FileName);
                            string _path = Path.Combine(Server.MapPath("~/Documents/SabbathDayMeditation"), _FileName);
                            string FilePath = "Documents/SabbathDayMeditation/" + _FileName;
                            if (db.Documents.Where(x => x.FilePath == FilePath).Count() == 0)
                            {
                                //file.SaveAs(_path);
                                //string imgurl = "";
                                //if (Doc.ImageUpload.ContentLength > 0)
                                //{
                                //    string _ImageName = Path.GetFileName(Doc.ImageUpload.FileName);
                                //    string _Imagepath = Path.Combine(Server.MapPath("~/Documents/DocumentImages"), _ImageName);
                                //    Doc.ImagesPath = "Documents/DocumentImages/" + _ImageName;
                                //    Doc.ImageUpload.SaveAs(_Imagepath);
                                //    imgurl = "To download this document Please <a href='http://www.magnifyelshaddai.com'><img src='" + _Imagepath + "' alt=''/>Click Here</a>";
                                //}
                                //else
                                //{
                                //    imgurl = "To download this document Please <a href='http://www.magnifyelshaddai.com'>Click Here</a>";
                                //}

                                //imgurl = "To download this document Please <a href='http://www.magnifyelshaddai.com'>Click Here</a>";

                                //var objDoc = new Document();
                                //objDoc.DocumentID = Doc.DocumentID;
                                //objDoc.CategoryId = Doc.CategoryId;
                                //objDoc.Title = Doc.Title;
                                //objDoc.FilePath = FilePath;
                                //objDoc.ImagesPath = Doc.ImagesPath;
                                //objDoc.DocumentReference = Doc.DocumentReference;
                                //objDoc.CreatedBy = Session["UserType"].ToString();
                                //objDoc.CreatedDateTime = DateTime.Now;
                                //objDoc.Status = true;
                                //db.Documents.Add(objDoc);
                                //db.SaveChanges();


                                //Email Send all

                                EmailModel obj = new EmailModel();

                                var EmailDB = db.EmailMessages.SingleOrDefault();
                                obj.ToEmail = "bibleworkshopteam@magnifyelshaddai.com";
                                obj.EmailSubject = "Bible Workshop Team - Test Mail";
                                //if (Doc.CategoryId == 1)
                                //{
                                //    Doc.EMailBody = "Sabbath day Document have been uploaded by Bible Workshop Team. <br/><br/>" + imgurl;
                                //}
                                //else
                                //{
                                //    Doc.EMailBody = "Document have been uploaded by Bible Workshop Team. <br/><br/>" + imgurl;
                                //}
                                //obj.EMailBody = Doc.EMailBody;
                                string Email = "<P>Praise the LORD Brother,</br></br>Greetings in the name of LORD Jesus Christ and Mother Mary.</br>" +
                                "<b>Bible workshop team</b> uploaded the sabbath day document in <a href='http://www.magnifyelshaddai.com/' target='_blank'>http://www.<wbr>magnifyelshaddai.com/</a>" +
                                "</br></br></br><b><i><font color='#0b5394'>Glory to the LORD Jesus Christ and Mother Mary.</font></i></b></br></br>" +
                                "<b><i><font color='#0b5394'>In Christ,</font></i></b></br>" +
                                "<b><i><font color='#0b5394'>Bible Workshop Team.</font></i></b></br></p>";
                                obj.EMailBody = Email;
                                obj.EmailCC = "";

                                //var emailList = db.Users.Where(x => x.UserId != 1 && x.IsActive == true && x.IsNotification == true).Select(u => u.Email).ToList();

                                var emailList = "edwinanbu08@gmail.com";

                                var emails = String.Join(",", emailList);

                                obj.EmailBCC = emails.ToString();

                                SmtpClient smtpClient = new SmtpClient();
                                smtpClient.Host = "relay-hosting.secureserver.net";
                                //smtpClient.Host = "smtpout.secureserver.net";
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

                                ViewBag.Message = "Email Sent Successfully.";


                                return RedirectToAction("DocumentIndex", "Admin");
                            }
                            else
                            {
                                ViewBag.Message = "This Document is already Save.!!";
                                return View(model);
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please login admin user!!";
                            return View(model);
                        }

                    }
                    else
                    {
                        ViewBag.Message = "File have not been uploaded!!";
                        return View(model);
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Message = "File upload failed!!" + ex;
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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

    }
}
