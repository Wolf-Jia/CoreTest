using Salmon.Common.ScLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            string path = Server.MapPath("~/Config/Env/Log4Net.config");
            
            LoggerFactory.ChannelLog.Info("Http Called.");

            return View();
        }

        //public ActionResult Admin()
        //{
        //    string apiUrl = Url.HttpRouteUrl("DefaultApi", new { contorller = "contracts" });
        //    ViewBag.ApiUrl = new Uri(Request.Url, apiUrl).AbsoluteUri.ToString();

        //    return View();
        //}
    }
}
