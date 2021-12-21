using Otorez.Models.ORM.Context.ProjectContext;
using Otorez.Models.ORM.Entity.AdminModels;
using Otorez.Models.ORM.Entity.SuperUserModels;
using Otorez.Models.TriggerEmail.SuperUserEmails;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Otorez.Controllers
{
    public class SuperUserController : Controller
    {

        //LogIn - LogOut Controllers
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(String EmailAddress, String PhoneNumber, String Password)
        {
            ProjectContext db = new ProjectContext(); ;

            var SuperUser = db.SuperUser.FirstOrDefault(x => x.EmailAddress == EmailAddress && x.PhoneNumber == PhoneNumber && x.Password == Password);

            if (SuperUser == null)
            {
                ViewBag.Error = "Kullanıcı Bulunamadı";

                return View();
            }


            else
            {
                //create a cookie
                HttpCookie OtorezSuperUserCookie = new HttpCookie("OtorezSuperUserCookie");
                //Add key-values in the cookie        
                OtorezSuperUserCookie.Values.Add("SuperUserID", SuperUser.SuperUserID.ToString());
                //set cookie expiry date-time. Made it to last for next 1 hours.
                OtorezSuperUserCookie.Expires = DateTime.Now.AddHours(1);
                //Most important, write the cookie to client.
                Response.Cookies.Add(OtorezSuperUserCookie);

                return RedirectToAction("Index", "SuperUser");
            }
        }


        public ActionResult LogOut()
        {

            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];
            OtorezSuperUserCookie.Expires = DateTime.Now.AddSeconds(0);
            Response.Cookies.Add(OtorezSuperUserCookie);

            return RedirectToAction("Login", "SuperUser");
        }
        //LogIn - LogOut Controllers





        //Index Controllers
        public ActionResult Index()
        {
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;
                ViewBag.FirstName = SuperUser.FirstName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                return View();
            }

            return View();
        }


        [HttpPost]
        public ActionResult Index(DateTime ReportStart, DateTime ReportEnd)
        {
            //Request Cookie
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {
                return RedirectToAction("Login", "SuperUser");

            }

            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;
                ViewBag.FirstName = SuperUser.FirstName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reports Main Query
                var Accounts = db.Account.Where(x => x.AccountCreationDate >= ReportStart && x.AccountCreationDate <= ReportEnd).ToList();

                //Report Intervals 
                var start = ReportStart.ToString("dd MMMM yyyy") + " " + "-";
                var end = ReportEnd.ToString("dd MMMM yyyy");
                ViewBag.ReportStart = start;
                ViewBag.ReportEnd = end;
                ViewBag.ReportTitle = "tarihleri arasındaki raporları görüntülemektesiniz!";

                //Reporting Nodes
                //Total Accounts
                var TotalAccounts = Accounts.Count();
                ViewBag.TotalAccounts = TotalAccounts;

                //Total Live Accounts 
                var TotalLiveAccounts = Accounts.Count(x => x.AccountStatus == "Live");
                ViewBag.TotalLiveAccounts = TotalLiveAccounts;

                //Total Demo Accounts
                var TotalDemoAccounts = Accounts.Count(x=> x.AccountStatus == "Demo");
                ViewBag.TotalDemoAccounts = TotalDemoAccounts;

                //Total Payments
                var PaymentAmount = Accounts.Sum(x => x.PaymentAmount);
                ViewBag.PaymentAmount = PaymentAmount;

                //Cancelled Accounts
                var CancelledAccounts = db.Account.Where(x => x.AccountStatus == "Cancelled");
                var TotalCancelledAccounts = CancelledAccounts.Count();
                ViewBag.TotalCancelledAccounts = TotalCancelledAccounts;

                //Cancel Percentage                  
                double CancelledPercentage = Convert.ToDouble(TotalCancelledAccounts) / Convert.ToDouble(TotalAccounts) * 100;
                ViewBag.CancelledPercentage = "Hesap İptal Oranı:" + " %" + CancelledPercentage;

                //Demo - Live Accounts Percentage
                double DemoLivePercentage = Convert.ToDouble(TotalDemoAccounts) / Convert.ToDouble(TotalLiveAccounts) * 100;
                ViewBag.DemoLivePercentage = "Demo/Canlı Hesap Oranı:" + " %" + DemoLivePercentage;



                return View();
            }

            return View();
        }
        //Index Controllers





        //CreateAccount Controllers
        public ActionResult CreateAccount()
        {
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Account Creation Date
                var AccountCreationDate = DateTime.Now;
                ViewBag.AccountCreationDate = AccountCreationDate;

           
                return View();
            }

            return View();
        }


        [HttpPost]
        public ActionResult CreateAccount(Account account)
        {

            //Request Cookie
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Account Creation Date
                var AccountCreationDate = DateTime.Now;
                ViewBag.AccountCreationDate = AccountCreationDate;

                var ExistingEmailAddress = db.Account.FirstOrDefault(x => x.EmailAddress == account.EmailAddress);

                if (account.AccountStatus == "Demo" && ExistingEmailAddress == null)

                {
                    //Demo Account Created
                    account.Password = account.FirstName + account.PhoneNumber;
                    account.DemoStartDate = account.AccountCreationDate;
                    account.DemoEndDate = account.AccountCreationDate?.AddDays(7);
                    db.Account.Add(account);
                    db.SaveChanges();

                  

                    //Demo Account Instructions Sent to User
                    var ToAddress = account.EmailAddress;
                    var body = new StringBuilder();
                    body.AppendLine("Merhaba " + account.FirstName + ",");
                    body.AppendLine();
                    body.AppendLine("Otorez'e kayıt olduğunuz için teşekkür ederiz!");
                    body.AppendLine();
                    body.AppendLine("Giriş bilgilerinizi aşağıda bulabilirsiniz;");
                    body.AppendLine();
                    body.AppendLine("Giriş Sayfası: " + "https://www.otorez.com/admin/login");
                    body.AppendLine();
                    body.AppendLine("Kullanıcı Adınız: " + account.EmailAddress);
                    body.AppendLine("Kullanıcı Şifreniz: " + account.Password);
                    body.AppendLine();

                    if(account.MembershipStatus == "Bronze")
                    {
                        body.AppendLine("Demo Üyelik Paketiniz: Maximum 10 Oda");
                    }
                    if (account.MembershipStatus == "Silver")
                    {
                        body.AppendLine("Demo Üyelik Paketiniz: Maximum 20 Oda");
                    }
                    if (account.MembershipStatus == "Gold")
                    {
                        body.AppendLine("Demo Üyelik Paketiniz: Maximum 30 Oda");
                    }

                    body.AppendLine("Demo Kullanım Başlangıç Tarihi: " + account.DemoStartDate?.ToString("dd MMMM yyyy"));
                    body.AppendLine("Demo Kullanım Bitiş Tarihi: " + account.DemoEndDate?.ToString("dd MMMM yyyy"));
                    body.AppendLine();
                    body.AppendLine("Saygılarımızla");
                    body.AppendLine("Otorez Ekibi");
                    TriggerNewAccountMail.SendAccountMail(body.ToString(), new System.Net.Mail.MailAddress(ToAddress));  

                    
                    ViewBag.AccountCreated = "Yeni Demo Hesap Yaratıldı";

                }


                else if (account.AccountStatus == "Live" && ExistingEmailAddress == null)
                {
                    //Live Account Created
                    account.Password = account.FirstName + account.PhoneNumber;
                    account.AccountStartDate = account.AccountCreationDate;
                    account.AccountEndDate = account.AccountCreationDate?.AddDays(365);
                    db.Account.Add(account);
                    db.SaveChanges();

                    //Live Account Instructions Sent to User
                    var ToAddress = account.EmailAddress;
                    var body = new StringBuilder();
                    body.AppendLine("Merhaba " + account.FirstName + ",");
                    body.AppendLine();
                    body.AppendLine("Otorez'e kayıt olduğunuz için teşekkür ederiz!");
                    body.AppendLine();
                    body.AppendLine("Giriş bilgilerinizi aşağıda bulabilirsiniz;");
                    body.AppendLine();
                    body.AppendLine("Giriş Sayfası: " + "https://www.otorez.com/admin/login");
                    body.AppendLine();
                    body.AppendLine("Kullanıcı Adınız: " + account.EmailAddress);
                    body.AppendLine("Kullanıcı Şifreniz: " + account.Password);
                    body.AppendLine();

                    if (account.MembershipStatus == "Bronze")
                    {
                        body.AppendLine("Üyelik Paketiniz: Maximum 10 Oda");
                    }
                    if (account.MembershipStatus == "Silver")
                    {
                        body.AppendLine("Üyelik Paketiniz: Maximum 20 Oda");
                    }
                    if (account.MembershipStatus == "Gold")
                    {
                        body.AppendLine("Üyelik Paketiniz: Maximum 30 Oda");
                    }

                    body.AppendLine("Hesap Kullanım Başlangıç Tarihi: " + account.AccountStartDate?.ToString("dd MMMM yyyy"));
                    body.AppendLine("Hesap Kullanım Bitiş Tarihi: " + account.AccountEndDate?.ToString("dd MMMM yyyy"));
                    body.AppendLine();
                    body.AppendLine("Saygılarımızla");
                    body.AppendLine("Otorez Ekibi");
                    TriggerNewAccountMail.SendAccountMail(body.ToString(), new System.Net.Mail.MailAddress(ToAddress));


                    ViewBag.AccountCreated = "Yeni Canlı Hesap Yaratıldı";

                }

                else
                {
                    ViewBag.ExistingEmailAddress = "Girilen email adresi başka bir kullanıcı tarafından kullanılıyor!";
                }
              
                return View();

            }

            return View();
        }
        //CreateAccount Controllers





        //UpdateAccount Controllers
        public ActionResult UpdateAccount(String EmailAddress, String PhoneNumber, DateTime? StartDate, DateTime? EndDate)
        {
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }

            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Search Accounts Main Query
                var Accounts = db.Account.Where(x => x.EmailAddress == EmailAddress || x.PhoneNumber == PhoneNumber || x.AccountCreationDate >= StartDate && x.AccountCreationDate <= EndDate).ToList();

                //Report Intervals             
                if (StartDate != null && EndDate != null)
                {
                    var start = StartDate?.ToString("dd MMMM yyyy") + " " + "-";
                    var end = EndDate?.ToString("dd MMMM yyyy");
                    ViewBag.ReportStart = start;
                    ViewBag.ReportEnd = end;
                    ViewBag.ReportTitle = "tarihleri arasındaki yaratılmış hesapları görüntülemektesiniz!";
                }
                //Report Intervals  

                //Update Account Selection Validation
                ViewBag.AccountNotSelected = TempData["AccountNotSelected"];
                //Account Updated
                ViewBag.AccountUpdated = TempData["AccountUpdated"];

                //Account Batches
                ViewBag.DemoBadge = "danger";
                ViewBag.DemoText = "Demo Hesap";
                ViewBag.LiveBadge = "success";
                ViewBag.LiveText = "Canlı Hesap";

                return View(Accounts);

            }

            return View();
        }


        public ActionResult GetSelected(String AccountID)
        {

            ProjectContext db = new ProjectContext();

            //Account ID Control for Updating Record
            if (AccountID == null)
            {
                TempData["AccountNotSelected"] = "Hesap güncelleme işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("SearchUpdate", "SuperUser");
            }

            else
            {
                var SelectedAccount = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                var SelectedAccountID = SelectedAccount.AccountID;
                TempData["SelectedAccountID"] = SelectedAccountID;

                return RedirectToAction("UpdateAccountRecord", "SuperUser");

            }

        }


        public ActionResult UpdateAccountRecord()
        {
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Selected Reservation Query
                var SelectedAccountID = TempData["SelectedAccountID"];
                var SelectedAccount = db.Account.FirstOrDefault(x => x.AccountID.ToString() == SelectedAccountID.ToString());


                return View(SelectedAccount);
            }

            return View();
        }


        [HttpPost]
        public ActionResult UpdateAccountRecord(Account account, String AccountID, DateTime? StartDate, DateTime? EndDate)
        {

            //Request Cookie
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            //Yes userId is found. Mission accomplished.
            string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

            //DB Connection
            ProjectContext db = new ProjectContext();

            //Pending Payments 
            var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
            var PendingPaymentCount = PendingPayment.Count();
            ViewBag.PendingPaymentCount = PendingPaymentCount;

            //Update Account Selection Main Query
            var SelectedAccount = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
 

            //Update Account Fields
            if (account.FirstName != null)
            {
                SelectedAccount.FirstName = account.FirstName;
                db.SaveChanges();
            }

            if (account.LastName != null)
            {
                SelectedAccount.LastName = account.LastName;

                db.SaveChanges();
            }

            if (account.PhoneNumber != null)
            {
                SelectedAccount.PhoneNumber = account.PhoneNumber;

                db.SaveChanges();
            }

            if (account.EmailAddress != null)
            {
                SelectedAccount.EmailAddress = account.EmailAddress;

                db.SaveChanges();
            }    

            if (account.CompanyName != null)
            {
                SelectedAccount.CompanyName = account.CompanyName;

                db.SaveChanges();
            }

            if (account.AccountStatus != null)
            {
                SelectedAccount.AccountStatus = account.AccountStatus;

                db.SaveChanges();
            }


            if (account.MembershipStatus != null)
            {
                SelectedAccount.MembershipStatus = account.MembershipStatus;

                db.SaveChanges();
            }

            if (account.AccountStartDate != null)
            {
                SelectedAccount.AccountStartDate = account.AccountStartDate;
                SelectedAccount.AccountEndDate = account.AccountStartDate?.AddDays(365);
                db.SaveChanges();
            }


            if (account.PaymentType != null)
            {
                SelectedAccount.PaymentType = account.PaymentType;

                db.SaveChanges();
            }

            if (account.PaymentStatus != null)
            {
                SelectedAccount.PaymentStatus = account.PaymentStatus;

                db.SaveChanges();
            }

            if (account.PaymentAmount != null)
            {
                SelectedAccount.PaymentAmount = account.PaymentAmount;

                db.SaveChanges();
            }

            if (account.Currency != null)
            {
                SelectedAccount.Currency = account.Currency;

                db.SaveChanges();
            }
         
            if (account.AccountStartDate != null && account.AccountStatus == "Live")
            {
                //Live Account Instructions Sent to User
                var ToAddress = SelectedAccount.EmailAddress;
                var body = new StringBuilder();
                body.AppendLine("Merhaba " + SelectedAccount.FirstName + ",");
                body.AppendLine();
                body.AppendLine("Otorez hesabınız güncellenmiştir!");
                body.AppendLine();
                body.AppendLine("Giriş bilgilerinizi aşağıda bulabilirsiniz;");
                body.AppendLine();
                body.AppendLine("Giriş Sayfası: " + "https://www.otorez.com/admin/login");
                body.AppendLine();
                body.AppendLine("Kullanıcı Adınız: " + SelectedAccount.EmailAddress);
                body.AppendLine("Kullanıcı Şifreniz: " + SelectedAccount.Password);
                body.AppendLine();

                if (account.MembershipStatus == "Bronze")
                {
                    body.AppendLine("Üyelik Paketiniz: Maximum 10 Oda");
                }
                if (account.MembershipStatus == "Silver")
                {
                    body.AppendLine("Üyelik Paketiniz: Maximum 20 Oda");
                }
                if (account.MembershipStatus == "Gold")
                {
                    body.AppendLine("Üyelik Paketiniz: Maximum 30 Oda");
                }

                body.AppendLine("Hesap Kullanım Başlangıç Tarihi: " + account.AccountStartDate?.ToString("dd MMMM yyyy"));
                body.AppendLine("Hesap Kullanım Bitiş Tarihi: " + account.AccountStartDate?.AddDays(365).ToString("dd MMMM yyyy"));
                body.AppendLine();
                body.AppendLine("Saygılarımızla");
                body.AppendLine("Otorez Ekibi");
                TriggerNewAccountMail.SendAccountMail(body.ToString(), new System.Net.Mail.MailAddress(ToAddress));
            }

            TempData["AccountUpdated"] = "Hesap Güncellenmiştir!";

            return RedirectToAction("UpdateAccount", "SuperUser");

        }
        //UpdateAccount Controllers




        //Payments Controllers
        public ActionResult Payments()
        {

            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;
                ViewBag.FirstName = SuperUser.FirstName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Account Batches
                ViewBag.DemoBadge = "danger";
                ViewBag.DemoText = "Demo Hesap";
                ViewBag.LiveBadge = "success";
                ViewBag.LiveText = "Canlı Hesap";

                return View(PendingPayment);

            }

            return View();
        }
        //Payments Controllers





        //Export Controllers
        public ActionResult Export()
        {
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            if (OtorezSuperUserCookie == null)
            {

                return RedirectToAction("Login", "SuperUser");

            }


            if (!string.IsNullOrEmpty(OtorezSuperUserCookie.Values["SuperUserID"]))

            {
                //Yes userId is found. Mission accomplished.
                string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //Super User Personalization
                var SuperUser = db.SuperUser.FirstOrDefault(x => x.SuperUserID.ToString() == SuperUserID);
                ViewBag.EmailAddress = SuperUser.EmailAddress;
                ViewBag.CompanyName = SuperUser.CompanyName;
                ViewBag.FirstName = SuperUser.FirstName;

                //Pending Payments 
                var PendingPayment = db.Account.Where(x => x.PaymentStatus != "Tamamlandı" && x.AccountStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Account Batches
                ViewBag.DemoBadge = "danger";
                ViewBag.DemoText = "Demo Hesap";
                ViewBag.LiveBadge = "success";
                ViewBag.LiveText = "Canlı Hesap";

                //Export Accounts Main Query
                var Accounts = db.Account.ToList();

                return View(Accounts);

            }

            return View();


        }


        public IEnumerable<Account> ExportGetAccounts()
        {
            //Request Cookie
            HttpCookie OtorezSuperUserCookie = Request.Cookies["OtorezSuperUserCookie"];

            //Yes userId is found. Mission accomplished.
            string SuperUserID = OtorezSuperUserCookie.Values["SuperUserID"];

            //DB Connection
            ProjectContext db = new ProjectContext();

            //Prepare All Accounts to Export Main Query
            var Accounts = db.Account.ToList();

            return Accounts;
        }


        public ActionResult ExportToExcel()
        {
            //Creating Grid View
            var gv = new GridView();
            gv.DataSource = this.ExportGetAccounts();
            gv.DataBind();

            //Response Paramaters
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Accounts-Report" + "-" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            Response.Charset = "UTF-8";

            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);

            gv.RenderControl(objHtmlTextWriter);

            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();

            return View("Export", "SuperUser");

        }
        //Export Controllers


        


    }
    }