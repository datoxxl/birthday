using Birthday.Domain;
using Birthday.Domain.Services;
using Birthday.Web.ActionFilters;
using Birthday.Web.Helpers;
using Birthday.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Birthday.Web.Controllers
{
    public class VisualizationController : BaseController
    {
        private BirthdayService _BirthdayService = new BirthdayService();

        public ActionResult VisualizationLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VisualizationLogin(VisualizationLogin model)
        {
            if (VisualizationLoginHelper.ValidateUser(model.Email, model.Password, Session))
            {
                SetVisualizationAccessCookie(model.Email, model.Password);

                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Invalid username/password!";
            return View(model);
        }

        [AccessActionFilter]
        public ActionResult Index()
        {
            return RedirectToAction("PersonInfo");
        }

        [AccessActionFilter]
        public ActionResult PersonInfo()
        {
            var birthday = _BirthdayService.Get(BirthdayID);
            var model = new PersonInfo();

            model.FirstName = birthday.FirstName;
            model.LastName = birthday.LastName;
            model.GenderID = (Genders?)birthday.GenderID;
            model.Age = birthday.Age;
            model.MobileNumber = birthday.MobileNumber;
            model.FacebookUrl = birthday.FacebookUrl;
            model.Email = birthday.Email;

            return View(model);
        }

        [AccessActionFilter]
        [HttpPost]
        public ActionResult PersonInfo(PersonInfo model)
        {
            if (ModelState.IsValid)
            {
                var birthday = _BirthdayService.Get(BirthdayID);

                birthday.FirstName = model.FirstName;
                birthday.LastName = model.LastName;
                birthday.GenderID = (int?)model.GenderID;
                birthday.Age = model.Age;
                birthday.MobileNumber = model.MobileNumber;
                birthday.FacebookUrl = model.FacebookUrl;
                birthday.Email = model.Email;

                _BirthdayService.Update(birthday);
                _BirthdayService.SaveChanges();
            }

            return RedirectToAction("Template");
        }

        [AccessActionFilter]
        public ActionResult Template()
        {
            var birthday = _BirthdayService.Get(BirthdayID);

            var templateList = new TemplateService(_BirthdayService).GetAll().ToList();

            var selectList = new SelectList(templateList, "TemplateID", "Title");

            var model = new TemplateSelect();

            model.TemplateList = selectList;
            model.TemplateID = birthday.TemplateID;

            return View(model);
        }

        [AccessActionFilter]
        [HttpPost]
        public ActionResult Template(TemplateSelect model)
        {
            if (ModelState.IsValid)
            {
                _BirthdayService.SetTemplate(BirthdayID, model.TemplateID.Value);

                return RedirectToAction("TemplateFill");
            }

            return View(model);
        }

        [AccessActionFilter]
        public ActionResult Complete()
        {
            ViewBag.IsPublished = _BirthdayService.GetBirthday(BirthdayID).Published;

            return View();
        }

        [HttpPost]
        public ActionResult Complete(FormCollection form)
        {
            var birthday = _BirthdayService.Get(BirthdayID);

            birthday.Published = true;

            _BirthdayService.Update(birthday);
            _BirthdayService.SaveChanges();

            return StatusView("Published successfully!");
        }

        [AccessActionFilter]
        public ActionResult TemplateFill()
        {
            var birthday = _BirthdayService.Get(BirthdayID);

            var model = new VisualizationViewModel { Html = birthday.Html };

            if (birthday.Template != null)
            {
                model.TemplateName = birthday.Template.Title;
            }

            model.ImageProps = ModelHelper.GetImageProps(birthday);
            model.Texts = ModelHelper.GetBirthdayTexts(birthday);

            return View(model);
        }

        [HttpPost, AccessActionFilter]
        public ActionResult TemplateFill(VisualizationViewModel model)
        {
            using (var service = new BirthdayImageService())
            {
                foreach (var item in model.ImageProps)
                {
                    service.UpdateImageProps(BirthdayID, item.Index, item.Left, item.Top, item.Width);
                }
            }

            foreach (var item in model.Texts)
            {
                _BirthdayService.UpdateBirthdayText(BirthdayID, item.Index, item.Text);
            }

            return RedirectToAction("Complete");
        }

        [HttpPost, AccessActionFilter]
        public JsonResult ImageUpload(int imageIndex)
        {
            try
            {
                var file = Request.Files[0];

                var content = new byte[file.InputStream.Length];

                file.InputStream.Read(content, 0, content.Length);

                using (var service = new BirthdayImageService())
                {
                    string errorMessage = null;

                    var image = service.SaveImage(content, file.ContentType, BirthdayID, imageIndex, UserID, ref errorMessage);
                    if (image != null)
                    {
                        return Json(new { image.ImageLeft, image.ImageTop, image.ImageWidth });
                    }
                    else
                    {
                        return JsonError(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public ImageResult GetBirthdayImage(int imageIndex)
        {
            using (var service = new BirthdayImageService())
            {
                var image = service.GetImage(BirthdayID, imageIndex);

                if (image == null || image.File == null)
                {
                    return new ImageResult(Server.MapPath("~/content/images/dot.png"));
                }

                return new ImageResult(new MemoryStream(image.File.Content), image.File.MimeType);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _BirthdayService.Dispose();
            base.Dispose(disposing);
        }
    }
}
