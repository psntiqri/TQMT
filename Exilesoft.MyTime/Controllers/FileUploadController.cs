using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Exilesoft.MyTime.Controllers
{
    public class FileUploadController : BaseController
    {
        private static string _lastUpdatedImage = string.Empty;

        public ActionResult Index()
        {
            ViewData["UploadImageScript"] = string.Empty;
            if (!_lastUpdatedImage.Equals(string.Empty))
            {
                ViewData["UploadImageScript"] = "<span id='SPAN_FileName'>" + _lastUpdatedImage + "</span>";
                _lastUpdatedImage = string.Empty;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SaveImage(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                FileInfo _fileInfo = new FileInfo(file.FileName);
                var fileName = string.Format(@"{0}{1}", Guid.NewGuid(), _fileInfo.Extension);
                var path = Path.Combine(Server.MapPath("~/Content/images/employee"), fileName);
                file.SaveAs(path);
                _lastUpdatedImage = fileName;
            }

            return RedirectToAction("Index");
        }
    }
}
