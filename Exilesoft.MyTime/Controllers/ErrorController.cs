using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Exilesoft.MyTime.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ViewResult Index()
        {
            return View();
        }
        public ViewResult NotFound()
        {
            return View("Error");
        }
    }
}