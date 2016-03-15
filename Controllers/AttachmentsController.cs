using BugTracker.Models;
using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Attachments
        public ActionResult Index()
        {
            return View();
        }

        //POST Attachments
        //[HttpPost]
        //public ActionResult AttachFile([Bind(Include="TicketId")] Attachment attachment, HttpPostedFileBase file)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (file == null)
        //        {
        //            ModelState.AddModelError("File", "Please Upload Your file");
        //        }
        //        else if (file.ContentLength > 0)
        //        {
        //            int MaxContentLength = 1024 * 1024 * 3; //3 MB
        //            string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf" };

        //            if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
        //            {
        //                ModelState.AddModelError("File", "Please file of type: " + string.Join(", ", AllowedFileExtensions));
        //            }

        //            else if (file.ContentLength > MaxContentLength)
        //            {
        //                ModelState.AddModelError("File", "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB");
        //            }
        //            else
        //            {
        //                //TO:DO
        //                attachment.Title = Path.GetFileName(file.FileName);
        //                var fileName = attachment.Title;
        //                attachment.FilePath = Path.Combine(Server.MapPath("~/Content/Upload"), fileName);
        //                var path = attachment.FilePath;
        //                file.SaveAs(path);
        //                ModelState.Clear();
        //                db.Attachments.Add(attachment);
        //                db.SaveChanges();
        //                ViewBag.Message = "File uploaded successfully";
        //            }
        //        }
        //    }
        //    //return RedirectToAction(,);
        //}
    }
}