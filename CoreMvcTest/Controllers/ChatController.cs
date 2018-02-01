using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreMvcTest.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.HistoryMessage = JsonConvert.SerializeObject(SocketHandler.historyMessage);
            return View();
        }
    }
}