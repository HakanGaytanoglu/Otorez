using Otorez.Models.ORM.Context.ProjectContext;
using Otorez.Models.ORM.Entity.HomeModels;
using Otorez.Models.TriggerEmail.HomeEmails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Otorez.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {

            var RegistrationDate = DateTime.Now;
            ViewBag.RegistrationDate = RegistrationDate;

            return View();
      
        }


        [HttpPost]
        public ActionResult Index (Registration registration, Newsletter newsletter)
        {
            ProjectContext db = new ProjectContext();

            if (ModelState.IsValid)
            {
                var RegistrationDate = DateTime.Now;
                ViewBag.RegistrationDate = RegistrationDate;

                db.Registration.Add(registration);
                db.SaveChanges();
               
                var body = new StringBuilder();
                body.AppendLine("Ad: " + registration.FirstName);
                body.AppendLine("Soyad: " + registration.LastName);
                body.AppendLine("E-Posta: " + registration.EmailAddress);
                body.AppendLine("Telefon: " + registration.PhoneNumber);
                body.AppendLine("Otel Adı: " + registration.CompanyName);
                body.AppendLine("Kayıt Tarihi: " + registration.RegistrationDate);

                TriggerRegistrationMail.SendMail(body.ToString());
                ViewBag.Success = "Talebiniz başarı ile alınmıştır. En kısa sürede sizinle iletişime geçilecektir.";

            }

            else
            {
                db.Newsletter.Add(newsletter);
                db.SaveChanges();
              

                var body = new StringBuilder();
                body.AppendLine("E-Posta: " + newsletter.LeadEmailAddress);
                TriggerNewsletterMail.SendMail(body.ToString());
                ViewBag.Success = "Bültenimize abone olduğunuz için teşekkür ederiz.";

            }

            return View();
           
        }
 


        public ActionResult Services()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Services(Newsletter newsletter)
        {
            ProjectContext db = new ProjectContext();

            db.Newsletter.Add(newsletter);
            db.SaveChanges();

            var body = new StringBuilder();
            body.AppendLine("E-Posta: " + newsletter.LeadEmailAddress);
            TriggerNewsletterMail.SendMail(body.ToString());
            ViewBag.Success = "Bültenimize abone olduğunuz için teşekkür ederiz.";

            return View();

        }


        public ActionResult Pricing()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Pricing(Newsletter newsletter)
        {
            ProjectContext db = new ProjectContext();

            db.Newsletter.Add(newsletter);
            db.SaveChanges();

            var body = new StringBuilder();
            body.AppendLine("E-Posta: " + newsletter.LeadEmailAddress);
            TriggerNewsletterMail.SendMail(body.ToString());
            ViewBag.Success = "Bültenimize abone olduğunuz için teşekkür ederiz.";

            return View();

        }


        public ActionResult Contact()
        {

            return View();
        }
  

        [HttpPost]
        public ActionResult Contact(ContactForm contactform, Newsletter newsletter)
        {
            ProjectContext db = new ProjectContext();

            if (ModelState.IsValid) {
            
                db.ContactForm.Add(contactform);
                db.SaveChanges();

                var body = new StringBuilder();
                body.AppendLine("Ad: " + contactform.FirstName);
                body.AppendLine("Soyad: " + contactform.LastName);
                body.AppendLine("E-Posta: " + contactform.EmailAddress);
                body.AppendLine("Telefon: " + contactform.PhoneNumber);
                body.AppendLine("Konu: " + contactform.Subject);
                body.AppendLine("Mesaj: " + contactform.Message);

                TriggerContactFormMail.SendMail(body.ToString());
                ViewBag.Success = "Talebiniz başarı ile alınmıştır. En kısa sürede sizinle iletişime geçilecektir.";

            }

            else
            {
                db.Newsletter.Add(newsletter);
                db.SaveChanges();

                var body = new StringBuilder();
                body.AppendLine("E-Posta: " + newsletter.LeadEmailAddress);
                TriggerNewsletterMail.SendMail(body.ToString());
                ViewBag.Success = "Bültenimize abone olduğunuz için teşekkür ederiz.";

            }

            return View();
        }
    }
}