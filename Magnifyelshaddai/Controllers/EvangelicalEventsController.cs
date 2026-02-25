using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magnifyelshaddai.Controllers
{
    public class EvangelicalEventsController : Controller
    {
        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                String path1 = Server.MapPath("~/Evangelical Events/2021/Poonamallee Melma Nagar Library");
                String path2 = Server.MapPath("~/Evangelical Events/2021/Poonamallee Melma Nagar Prayer");
                String path3 = Server.MapPath("~/Evangelical Events/2022/Athukudi Prayer");
                String path4 = Server.MapPath("~/Evangelical Events/2022/Medavakkam Bible Expo");
                String path5 = Server.MapPath("~/Evangelical Events/2022/Perungudi Church Prayer");
                String path6 = Server.MapPath("~/Evangelical Events/2022/Tirunelveli Retreat");                
                String path7 = Server.MapPath("~/Evangelical Events/2023/Mercy 2023");
                String path8 = Server.MapPath("~/Evangelical Events/2023/Dindigul Mullipadi Prayer");
                String[] imagesfiles1 = Directory.GetFiles(path1);
                String[] imagesfiles2 = Directory.GetFiles(path2);
                String[] imagesfiles3 = Directory.GetFiles(path3);
                String[] imagesfiles4 = Directory.GetFiles(path4);
                String[] imagesfiles5 = Directory.GetFiles(path5);
                String[] imagesfiles6 = Directory.GetFiles(path6);
                String[] imagesfiles7 = Directory.GetFiles(path7);
                String[] imagesfiles8 = Directory.GetFiles(path8);

                ViewBag.images1 = imagesfiles1;
                ViewBag.images2 = imagesfiles2;
                ViewBag.images3 = imagesfiles3;
                ViewBag.images4 = imagesfiles4;
                ViewBag.images5 = imagesfiles5;
                ViewBag.images6 = imagesfiles6;
                ViewBag.images7 = imagesfiles7;
                ViewBag.images8 = imagesfiles8;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Authentication");
            }
        }
    }
}
