using Otorez.KbsService;
using Otorez.Models.ORM.Context.ProjectContext;
using Otorez.Models.ORM.Entity.AdminModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Otorez.Controllers
{
    public class AdminController : Controller
    {

        //LogIn - LogOut Controllers
        public ActionResult Login()
        {
           
            ViewBag.AccountPasswordUpdated = TempData["AccountPasswordUpdated"];
            
            return View();        
        }


        [HttpPost]
        public ActionResult Login(String EmailAddress, String Password)
        {
            ProjectContext db = new ProjectContext(); ;

            var user = db.Account.FirstOrDefault(x => x.EmailAddress == EmailAddress && x.Password == Password);

            if (user == null)
            {
                ViewBag.Error = "Kullanıcı Bulunamadı";

                return View();
            }
  

            else
            {
                if (user.AccountStatus == "Demo" && user.DemoEndDate > DateTime.Now)
                {
                    //create a cookie
                    HttpCookie OtorezCookie = new HttpCookie("OtorezCookie");
                    //Add key-values in the cookie        
                    OtorezCookie.Values.Add("AccountID", user.AccountID.ToString());
                    //set cookie expiry date-time. Made it to last for next 1 hours.
                    OtorezCookie.Expires = DateTime.Now.AddHours(1);
                    //Most important, write the cookie to client.
                    Response.Cookies.Add(OtorezCookie);

                }

                else if (user.AccountStatus == "Live" && user.AccountEndDate > DateTime.Now)
                {
                    //create a cookie
                    HttpCookie OtorezCookie = new HttpCookie("OtorezCookie");
                    //Add key-values in the cookie        
                    OtorezCookie.Values.Add("AccountID", user.AccountID.ToString());
                    //set cookie expiry date-time. Made it to last for next 1 hours.
                    OtorezCookie.Expires = DateTime.Now.AddHours(1);
                    //Most important, write the cookie to client.
                    Response.Cookies.Add(OtorezCookie);
        
                }

                else
                {
                    ViewBag.AccountExpired1 = "Kullanmakta olduğunuz Otorez hesabınızın kullanım süresi dolmuştur.";
                    ViewBag.AccountExpired2 = "Hesabınızı kullanmaya devam etmek için lütfen Otorez destek ekibi ile iletişime geçiniz..";
                    return View();
                }
         

                return RedirectToAction("Index", "Admin");
            }
         

        }


        public ActionResult LogOut()
        {

            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];
            OtorezCookie.Expires = DateTime.Now.AddSeconds(0);
            Response.Cookies.Add(OtorezCookie);

            return RedirectToAction ("Login", "Admin");
        }
        //LogIn - LogOut Controllers






        //Index Controllers
        public ActionResult Index()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];    

            if (OtorezCookie == null)
            {

                 return RedirectToAction("Login", "Admin");

            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
                
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;
                ViewBag.FirstName = user.FirstName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
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
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");

            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;
                ViewBag.FirstName = user.FirstName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reports Main Query
                var Reservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.CheckIn >= ReportStart && x.CheckOut <= ReportEnd).ToList();

                //Report Intervals 
                var start = ReportStart.ToString("dd MMMM yyyy") + " " + "-";
                var end = ReportEnd.ToString("dd MMMM yyyy");
                ViewBag.ReportStart = start;
                ViewBag.ReportEnd = end;
                ViewBag.ReportTitle = "tarihleri arasındaki raporları görüntülemektesiniz!";

                //Reporting Nodes
                //Total Reservation
                var TotalReservations = Reservations.Count();
                ViewBag.TotalReservations = TotalReservations;

                //Sold Total Rooms Count in the period
                var TotalNights = Reservations.Sum(x => x.TotalNights);
                ViewBag.TotalNights = TotalNights;

                //Total Guest
                var TotalAdults = Reservations.Sum(x => x.TotalNumberAdults);
                var TotalKids = Reservations.Sum(x => x.TotalNumberKids);
                var TotalGuests = TotalAdults + TotalKids;
                ViewBag.TotalGuests = TotalGuests;
              
                //Total Income
                var TotalIncome = Reservations.Sum(x => x.PaymentAmount);
                ViewBag.TotalIncome = TotalIncome;
               
                //Fullness Percentage
                //Count of Each Rooms
                var Single = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Single Room").ToList();
                if (Single != null)
                {
                    var SingleQuantity = Single.Sum(x => x.RoomCount);
                    TempData["SingleQuantity"] = SingleQuantity;
                }
                var Double = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Double Room").ToList();
                if (Double != null)
                {
                    var DoubleQuantity = Double.Sum(x => x.RoomCount);
                    TempData["DoubleQuantity"] = DoubleQuantity;
                }
                var Twin = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Twin Room").ToList();
                if (Twin != null)
                {
                    var TwinQuantity = Twin.Sum(x => x.RoomCount);
                    TempData["TwinQuantity"] = TwinQuantity;
                }
                var Triple = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Triple Room").ToList();
                if (Triple != null)
                {
                    var TripleQuantity = Triple.Sum(x => x.RoomCount);
                    TempData["TripleQuantity"] = TripleQuantity;
                }
                var Quad = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Quad Room").ToList();
                if (Quad != null)
                {
                    var QuadQuantity = Quad.Sum(x => x.RoomCount);
                    TempData["QuadQuantity"] = QuadQuantity;
                }
                var Family = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Family Room").ToList();
                if (Family != null)
                {
                    var FamilyQuantity = Family.Sum(x => x.RoomCount);
                    TempData["FamilyQuantity"] = FamilyQuantity;
                }
                var Suit = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Suit Room").ToList();
                if (Suit != null)
                {
                    var SuitQuantity = Suit.Sum(x => x.RoomCount);
                    TempData["SuitQuantity"] = SuitQuantity;
                }
                var Bungalow = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Bungalow").ToList();
                if (Bungalow != null)
                {
                    var BungalowQuantity = Bungalow.Sum(x => x.RoomCount);
                    TempData["BungalowQuantity"] = BungalowQuantity;
                }
                var Villa = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Villa").ToList();
                if (Villa != null)
                {
                    var VillaQuantity = Villa.Sum(x => x.RoomCount);
                    TempData["VillaQuantity"] = VillaQuantity;
                }

                var FinalSingleQuantity = TempData["SingleQuantity"];
                var FinalDoubleQuantity = TempData["DoubleQuantity"];
                var FinalTwinQuantity = TempData["TwinQuantity"];
                var FinalTripleQuantity = TempData["TripleQuantity"];
                var FinalQuadQuantity = TempData["QuadQuantity"];
                var FinalFamilyQuantity = TempData["FamilyQuantity"];
                var FinalSuitQuantity = TempData["SuitQuantity"];
                var FinalBungalowQuantity = TempData["BungalowQuantity"];
                var FinalVillaQuantity = TempData["VillaQuantity"];

                var StaticTotalRoomCount = Convert.ToInt32(FinalSingleQuantity) + Convert.ToInt32(FinalDoubleQuantity) + Convert.ToInt32(FinalTwinQuantity) + Convert.ToInt32(FinalTripleQuantity) + Convert.ToInt32(FinalQuadQuantity) + Convert.ToInt32(FinalFamilyQuantity) + Convert.ToInt32(FinalSuitQuantity) + Convert.ToInt32(FinalBungalowQuantity) + Convert.ToInt32(FinalVillaQuantity);
          
                //Sellable Total Rooms Count in the period
                var Period = (ReportEnd - ReportStart).Days.ToString();
                var TotalRoomsCount = StaticTotalRoomCount * Convert.ToInt32(Period);
                ViewBag.TotalRoomsCount = TotalRoomsCount;

                //Fullness Calculation
                double Result = Convert.ToDouble(TotalNights) / Convert.ToDouble(TotalRoomsCount) * 100;
                var Fullness = Result.ToString("F");

                //Fullness View Result
                ViewBag.Fullness = "%" + " " + Fullness;
                ViewBag.ChartFullness = "Doluluk Oranı:" + " " + "%" + Fullness;

                //Cancelled Reservations
                var CancelledReservation = Reservations.Where(x => x.ReservationStatus == "Cancelled");
                var TotalCancelledReservations = CancelledReservation.Count();
                ViewBag.TotalCancelledReservations = TotalCancelledReservations;

                //Cancel Percentage                  
                double CancelledPercentage = Convert.ToDouble(TotalCancelledReservations) / Convert.ToDouble(TotalReservations) * 100;
                ViewBag.CancelledPercentage = "Rezervasyon İptal Oranı:" + " %" + CancelledPercentage;



                return View();
            }

            return View();
        }
        //Index Controllers





        //New Record Controllers
        public ActionResult NewRecord()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            //Reservation TimeStamp
            var ReservationDate = DateTime.Now;
            ViewBag.ReservationDate = ReservationDate;


            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
              
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;     

                //New Reservation Data - Search Result
                ViewBag.RoomNumber = TempData["NewReservationRoomNumber"];
                ViewBag.RoomType = TempData["NewReservationRoomType"];
                ViewBag.CheckIn = TempData["NewReservationCheckIn"];
                ViewBag.CheckOut = TempData["NewReservationCheckOut"];

                return View();

            }

            return View();
        }

        public ActionResult NewRecordGetSelected(String RoomNumber, String RoomType, String NewReservationCheckIn, String NewReservationCheckOut)
        {

            ProjectContext db = new ProjectContext();

            //RoomNumber Control for passing data to new record controller
            if (RoomNumber == null)
            {
                TempData["RoomNotSelected"] = "Rezervasyon yapmak için müsait bir oda seçmelisiniz!";
                return RedirectToAction("SearchAvailability", "Admin");
            }

            else
            {
                var NewReservationRoomNumber = RoomNumber;
                TempData["NewReservationRoomNumber"] = NewReservationRoomNumber;

                var NewReservationRoomType = RoomType;
                TempData["NewReservationRoomType"] = NewReservationRoomType;

                var NewResCheckIn = NewReservationCheckIn;
                TempData["NewReservationCheckIn"] = NewResCheckIn;

                var NewResCheckOut = NewReservationCheckOut;
                TempData["NewReservationCheckOut"] = NewResCheckOut;

                return RedirectToAction("NewRecord", "Admin");

            }

        }


        [HttpPost]
        public ActionResult NewRecord(Reservation reservation, DateTime? CheckIn, DateTime? CheckOut)
        {
     
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
                
                ProjectContext db = new ProjectContext();
          
                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;
              
                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //CheckIn - CheckOut Validation
                var ReservationDate = DateTime.Now.AddDays(-1);

                if (CheckIn < ReservationDate)
                {
                    ViewBag.PastDateError = "Otele giriş tarihi bugünden önceki bir tarih olamaz";
                }

                else if (ModelState.IsValid && CheckIn < CheckOut)
                {
                    var TotalNights = (CheckOut - CheckIn).Value.Days.ToString();
                    reservation.TotalNights = Convert.ToInt32(TotalNights);
                    db.Reservation.Add(reservation);
                    db.SaveChanges();
                    ViewBag.ReservationAdded = "Rezervasyon kaydedildi!";            
                }          
                
                else
                {
                    ViewBag.DateError = "Otele giriş tarihi çıkış tarihinden önce olmalıdır!";          
                }


                return View();
            }
               
            return View();
        }
        //New Record Controllers





        //Search Availability Controllers
        public ActionResult SearchAvailability(DateTime? CheckIn, DateTime? CheckOut, String RoomType)
        {

            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];           

            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
              
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Report Intervals             
                if (CheckIn != null && CheckOut != null) { 
                var start = CheckIn?.ToString("dd MMMM yyyy");
                var end = CheckOut?.ToString("dd MMMM yyyy");
                ViewBag.ReportStart = start;
                ViewBag.ReportEnd = end;
                ViewBag.Seperator = " - ";
                ViewBag.ReportTitle = "tarihleri arasındaki müsaitlik durumunu görüntülemektesiniz!";
                }

                //New Reservation Dates 
                if (CheckIn != null && CheckOut != null)
                {
                var NewReservationCheckIn = CheckIn?.ToString("yyyy-MM-dd");
                var NewREservationCheckOut = CheckOut?.ToString("yyyy-MM-dd");
                ViewBag.NewReservationCheckIn = NewReservationCheckIn;
                ViewBag.NewREservationCheckOut = NewREservationCheckOut;
                var AvailableRoomCheckIn = CheckIn?.ToString("dd/MM/yyyy");
                var AvailableRoomCheckOut = CheckOut?.ToString("dd/MM/yyyy");
                ViewBag.AvailableRoomCheckIn = AvailableRoomCheckIn;
                ViewBag.AvailableRoomCheckOut = AvailableRoomCheckOut;

                }

                //RoomTypes DropDown 
                var RoomTypes = db.Room.Where(x => x.AccountID == user.AccountID).GroupBy(x => new { x.RoomType }).Select(x => x.FirstOrDefault());
                ViewBag.RoomTypes = RoomTypes;


                //CheckIn - CheckOut Validation
                var ReservationDate = DateTime.Now.AddDays(-1);
                ViewBag.PastDateError = TempData["PastDateError"];
                ViewBag.DateError = TempData["DateError"];

                if (CheckIn < ReservationDate)
                {
                    TempData["PastDateError"] = "Otele giriş tarihi bugünden önceki bir tarih olamaz !";
                    return RedirectToAction("SearchAvailability", "Admin");
                }

                else if (CheckIn == null || CheckIn < CheckOut)
                {

                    //Search Availability Main Queries
                    //Reservations Falling Between Query Dates
                    var Reservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && CheckIn >= x.CheckIn && CheckIn < x.CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled" || x.AccountID.ToString() == AccountID && CheckOut > x.CheckIn && CheckOut <= x.CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled" || x.AccountID.ToString() == AccountID && x.CheckIn >= CheckIn && x.CheckOut <= CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled").ToList();
                    var ReservationsCount = Reservations.Count();

                    //Selected Room
                    var SelectedRoom = db.Room.Where(x => x.AccountID.ToString() == AccountID && x.RoomType == RoomType).ToList();

                    //Available Room Door Numbers
                    var BookedRoomNumbers = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && CheckIn >= x.CheckIn && CheckIn < x.CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled" || x.AccountID.ToString() == AccountID && CheckOut > x.CheckIn && CheckOut <= x.CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled" || x.AccountID.ToString() == AccountID && x.CheckIn >= CheckIn && x.CheckOut <= CheckOut && x.RoomType == RoomType && x.ReservationStatus != "Cancelled").Select(x => x.RoomNumber).ToList();
                    var AllRoomNumbers = db.Room.Where(x => x.AccountID.ToString() == AccountID && x.RoomType == RoomType).Select(x => x.RoomNumber).ToList();
                    var AvailableRoomNumbers = AllRoomNumbers.Except(BookedRoomNumbers);
                    ViewBag.AvailableRoomNumbers = AvailableRoomNumbers;
              

                //New Reservation Validation
                ViewBag.RoomNotSelected = TempData["RoomNotSelected"];

                //Available Rooms Calculations
                ViewBag.RoomType = RoomType;

                if (RoomType == "Single Room")
                {
                    var TotalSingleRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalSingleRoomCount = TotalSingleRoomCount;

                    var AvailableSingleRoomCount = TotalSingleRoomCount - ReservationsCount;
                    ViewBag.AvailableSingleRoomCount = AvailableSingleRoomCount;


                    if (ReservationsCount < TotalSingleRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Double Room")
                {
                    var TotalDoubleRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalDoubleRoomCount = TotalDoubleRoomCount;

                    var AvailableDoubleRoomCount = TotalDoubleRoomCount - ReservationsCount;
                    ViewBag.AvailableDoubleRoomCount = AvailableDoubleRoomCount;

                    if (ReservationsCount < TotalDoubleRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Twin Room")
                {
                    var TotalTwinRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalTwinRoomCount = TotalTwinRoomCount;

                    var AvailableTwinRoomCount = TotalTwinRoomCount - ReservationsCount;
                    ViewBag.AvailableTwinRoomCount = AvailableTwinRoomCount;

                    if (ReservationsCount < TotalTwinRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Triple Room")
                {
                    var TotalTripleRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalTripleRoomCount = TotalTripleRoomCount;

                    var AvailableTripleRoomCount = TotalTripleRoomCount - ReservationsCount;
                    ViewBag.AvailableTripleRoomCount = AvailableTripleRoomCount;

                    if (ReservationsCount < TotalTripleRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }


                if (RoomType == "Quad Room")
                {
                    var TotalQuadRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalQuadRoomCount = TotalQuadRoomCount;

                    var AvailableQuadRoomCount = TotalQuadRoomCount - ReservationsCount;
                    ViewBag.AvailableQuadRoomCount = AvailableQuadRoomCount;

                    if (ReservationsCount < TotalQuadRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Family Room")
                {
                    var TotalFamilyRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalFamilyRoomCount = TotalFamilyRoomCount;

                    var AvailableFamilyRoomCount = TotalFamilyRoomCount - ReservationsCount;
                    ViewBag.AvailableFamilyRoomCount = AvailableFamilyRoomCount;

                    if (ReservationsCount < TotalFamilyRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Suit Room")
                {
                    var TotalSuitRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalSuitRoomCount = TotalSuitRoomCount;

                    var AvailableSuitRoomCount = TotalSuitRoomCount - ReservationsCount;
                    ViewBag.AvailableSuitRoomCount = AvailableSuitRoomCount;

                    if (ReservationsCount < TotalSuitRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Suit Room")
                {
                    var TotalSuitRoomCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalSuitRoomCount = TotalSuitRoomCount;

                    var AvailableSuitRoomCount = TotalSuitRoomCount - ReservationsCount;
                    ViewBag.AvailableSuitRoomCount = AvailableSuitRoomCount;

                    if (ReservationsCount < TotalSuitRoomCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Bungalow")
                {
                    var TotalBungalowCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalBungalowCount = TotalBungalowCount;

                    var AvailableBungalowCount = TotalBungalowCount - ReservationsCount;
                    ViewBag.AvailableBungalowCount = AvailableBungalowCount;

                    if (ReservationsCount < TotalBungalowCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                if (RoomType == "Villa")
                {
                    var TotalVillaCount = SelectedRoom.Sum(x => x.RoomCount);
                    ViewBag.TotalVillaCount = TotalVillaCount;

                    var AvailableVillaCount = TotalVillaCount - ReservationsCount;
                    ViewBag.AvailableVillaCount = AvailableVillaCount;

                    if (ReservationsCount < TotalVillaCount)
                    {
                        ViewBag.Availability = "Müsait";
                        ViewBag.Badge = "success";
                    }

                    else
                    {
                        ViewBag.Availability = "Müsaitlik Yok";
                        ViewBag.Badge = "danger";
                    }
                }

                return View(Reservations);

                }


                else
                {

                    TempData["DateError"] = "Otele giriş tarihi çıkış tarihinden önce olmalıdır!";
                    return RedirectToAction("SearchAvailability", "Admin");
                   
                }


            }

            return View();
        }
        //Search Availability Controllers





        //Search Update Controllers
        public ActionResult SearchUpdate(String FirstName, String LastName, DateTime? CheckIn, DateTime? CheckOut)
        {

            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
               
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);              
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notifications
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Search Reservations Main Query
                var Reservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.CheckIn >= CheckIn && x.CheckOut <= CheckOut || x.AccountID.ToString() == AccountID && x.CheckIn == CheckIn || x.AccountID.ToString() == AccountID && x.CheckOut == CheckOut || x.AccountID.ToString() == AccountID && x.FirstName == FirstName || x.AccountID.ToString() == AccountID && x.LastName == LastName).OrderByDescending(s => s.CheckIn).ToList();

                //Report Intervals             
                if (CheckIn != null && CheckOut != null)
                {
                    var start = CheckIn?.ToString("dd MMMM yyyy") + " " + "-";
                    var end = CheckOut?.ToString("dd MMMM yyyy");
                    ViewBag.ReportStart = start;
                    ViewBag.ReportEnd = end;
                    ViewBag.ReportTitle = "tarihleri arasındaki rezervasyonları görüntülemektesiniz!";
                }             
                //Report Intervals  

                //Update Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];
                //Update Validation
                ViewBag.ReservationUpdated = TempData["ReservationUpdated"];

                //Cancelled Reservations Batch
                ViewBag.Badge = "danger";
                ViewBag.Text = "İptal Edildi";

                return View(Reservations);

            }
    
            return View();
        }


        public ActionResult SearchUpdateGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyon güncelleme işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("SearchUpdate", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;

                return RedirectToAction("SearchUpdateRecord", "Admin");

            }
        
        }


        public ActionResult SearchUpdateRecord()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
                
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;
        
                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //RoomTypes DropDown 
                var RoomTypes = db.Room.Where(x => x.AccountID == user.AccountID).GroupBy(x => new { x.RoomType }).Select(x => x.FirstOrDefault());
                ViewBag.RoomTypes = RoomTypes;

                //Selected Reservation Query
                var SelectedReservationID = TempData["SelectedReservationID"];
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == SelectedReservationID.ToString());
              

                return View(SelectedReservation);

            }

            return View();


        }


        [HttpPost]
        public ActionResult SearchUpdateRecord(Reservation reservation, String ReservationID, DateTime? CheckIn, DateTime? CheckOut)
        {

            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            //Yes userId is found. Mission accomplished.
            string AccountID = OtorezCookie.Values["AccountID"];

            //DB Connection
            ProjectContext db = new ProjectContext();

            //Pending Payment Notification
            var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
            var PendingPaymentCount = PendingPayment.Count();
            ViewBag.PendingPaymentCount = PendingPaymentCount;

            //Update Reservation Selection Main Query
            var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);

            //Update Reservation Fields
            if (reservation.FirstName != null)
            {
                SelectedReservation.FirstName = reservation.FirstName;
                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.LastName != null)
            {
                SelectedReservation.LastName = reservation.LastName;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.CheckIn != null)
            {
                SelectedReservation.CheckIn = reservation.CheckIn;

                var TotalNights = (SelectedReservation.CheckOut - CheckIn).Value.Days.ToString();
                SelectedReservation.TotalNights = Convert.ToInt32(TotalNights);

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.CheckOut != null)
            {
                SelectedReservation.CheckOut = reservation.CheckOut;

                var TotalNights = (CheckOut - SelectedReservation.CheckIn).Value.Days.ToString();
                SelectedReservation.TotalNights = Convert.ToInt32(TotalNights);

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.MobileNumber != null)
            {
                SelectedReservation.MobileNumber = reservation.MobileNumber;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.EmailAddress != null)
            {
                SelectedReservation.EmailAddress = reservation.EmailAddress;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.TotalNumberAdults != null)
            {
                SelectedReservation.TotalNumberAdults = reservation.TotalNumberAdults;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.TotalNumberKids != null)
            {
                SelectedReservation.TotalNumberKids = reservation.TotalNumberKids;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }


            if (reservation.RoomType != null)
            {
                SelectedReservation.RoomType = reservation.RoomType;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.RoomNumber != null)
            {
                SelectedReservation.RoomNumber = reservation.RoomNumber;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }


            if (reservation.BoardType != null)
            {
                SelectedReservation.BoardType = reservation.BoardType;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.PaymentType != null)
            {
                SelectedReservation.PaymentType = reservation.PaymentType;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.PaymentStatus != null)
            {
                SelectedReservation.PaymentStatus = reservation.PaymentStatus;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.PaymentAmount != null)
            {
                SelectedReservation.PaymentAmount = reservation.PaymentAmount;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.Currency != null)
            {
                SelectedReservation.Currency = reservation.Currency;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.Pets != null)
            {
                SelectedReservation.Pets = reservation.Pets;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (reservation.ReservationStatus != null)
            {
                SelectedReservation.ReservationStatus = reservation.ReservationStatus;

                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            if (CheckIn != null && CheckOut !=null)
            {
                var TotalNights = (CheckOut - CheckIn).Value.Days.ToString();
                SelectedReservation.TotalNights = Convert.ToInt32(TotalNights);
                db.SaveChanges();
                TempData["ReservationUpdated"] = "Rezervasyon Güncellenmiştir!";
            }

            
            return RedirectToAction("SearchUpdate", "Admin");

        }
        //Search Update Controllers





        //Payment Controllers
        public ActionResult Payments()
        {

            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");

            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                return View(PendingPayment);

            }

            return View();

        }
        //Payment Controllers


        


        //CheckOut Controllers 
        public ActionResult CheckOut(DateTime? CheckOut)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");

            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Update Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];

                //CheckOut Main Query
                var CheckOutReservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.CheckOut == CheckOut && x.ReservationStatus != "Cancelled").ToList();

                //Record Select Validation
                if(CheckOut != null && !CheckOutReservations.Any())
                {
                    ViewBag.NoReservations = "Seçmiş olduğunuz tarihte çıkış yapacak oda bulunmamaktadır.";
                }
             

                return View(CheckOutReservations);

            }

            return View();
        }


        public ActionResult CheckOutGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Çıkış işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("CheckOut", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;

                return RedirectToAction("CheckOutRecord", "Admin");

            }

        }


        public ActionResult CheckOutRecord()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");

            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payments 
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //CheckOut Reservation Main Query
                var ReservationID = TempData["SelectedReservationID"];
                var Reservation = db.Reservation.FirstOrDefault(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString());

                //CheckOut Services Main Query
                var Minibar = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Minibar").ToList();
                ViewBag.Minibar = Minibar;
                var RoomService = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Oda Servisi").ToList();
                ViewBag.RoomService = RoomService;
                var RestaurantCafe = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Resturant / Cafe").ToList();
                ViewBag.RestaurantCafe = RestaurantCafe;

                var Services = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString()).ToList();
                var TotalServicePrice = Services.Sum(x => x.TotalPrice);
                ViewBag.TotalServicePrice = TotalServicePrice + " TL";

                return View(Reservation);

            }


            return View();
        }
        //CheckOut Controllers 





        //Minibar Controllers 
        public ActionResult Minibar(String RoomNumber)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Main Query for Getting Reservation
                var Today = DateTime.Today;
                var Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && Today >= x.CheckIn && Today <= x.CheckOut && x.RoomNumber == RoomNumber && x.ReservationStatus != "Cancelled").ToList();
                    
                //Update Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];

                if (RoomNumber != null && !Reservation.Any())
                {
                    ViewBag.NoRoom = "Seçmiş olduğunuz odada rezervasyon bulunmamaktadır";
                }



                return View(Reservation);

            }

            return View();
        }


        public ActionResult MinibarGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyona ürün ekleme işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("Minibar", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;
                return RedirectToAction("MinibarAddProduct", "Admin");
            }

        }


        public ActionResult MinibarAddProduct()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData["SelectedReservationID"];
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString()).ToList();

                //Main Query for Getting Minibar Products
                var MinibarProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Minibar").ToList();
                        
                //Products to be removed from Reservation
                var AddedMinibarProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Minibar").ToList();
                ViewBag.AddedMinibarProducts = AddedMinibarProducts;


                return View(MinibarProducts);

            }

            return View();
        }


        [HttpPost]
        public ActionResult MinibarAddProduct(ServicePurchase servicePurchase, String ReservationID)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var CurrentReservationID = ReservationID;
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == CurrentReservationID.ToString()).ToList();
                TempData["SelectedReservationID"] = ReservationID;

                //Main Query for Getting Minibar Products
                var MinibarProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Minibar").ToList();
             
                //Products to be removed from Reservation
                var AddedMinibarProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Minibar").ToList();
                ViewBag.AddedMinibarProducts = AddedMinibarProducts;

                //Add ServicePurchase Item
                var itemTotalPrice = servicePurchase.ProductPrice * servicePurchase.Quantity;
                servicePurchase.TotalPrice = itemTotalPrice;

                var PurchaseDate = DateTime.Now;
                servicePurchase.PurchaseDate = PurchaseDate;

                db.ServicePurchase.Add(servicePurchase);
                db.SaveChanges();
               

                return RedirectToAction("MinibarAddProduct", "Admin");

            }

            return View();
        }


        public ActionResult DeleteMinibarProducts(IEnumerable<int> ServicePurchaseID, String ReservationID)
        {
            ProjectContext db = new ProjectContext();

            if (ServicePurchaseID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir kayıt seçmelisiniz !.";
            }

            else
            {

                foreach (var item in ServicePurchaseID)
                {

                    var delete = db.ServicePurchase.FirstOrDefault(x => x.ServicePurchaseID == item);
                    TempData["SelectedReservationID"] = ReservationID;

                    if (delete != null)
                    {
                        db.ServicePurchase.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }

            return RedirectToAction("MinibarAddProduct", "Admin");
        }
        //Minibar Controllers 





        //Room Service Controllers
        public ActionResult RoomService(String RoomNumber)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Main Query for Getting Reservation
                var Today = DateTime.Today;
                var Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && Today >= x.CheckIn && Today <= x.CheckOut && x.RoomNumber == RoomNumber && x.ReservationStatus != "Cancelled").ToList();

                //Update Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];

                if (RoomNumber != null && !Reservation.Any())
                {
                    ViewBag.NoRoom = "Seçmiş olduğunuz odada rezervasyon bulunmamaktadır";
                }



                return View(Reservation);

            }

            return View();
        }

        public ActionResult RoomServiceGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyona oda servisi ekleme işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("RoomService", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;
                return RedirectToAction("RoomServiceAddProduct", "Admin");
            }

        }


        public ActionResult RoomServiceAddProduct()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData["SelectedReservationID"];
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString()).ToList();

                //Main Query for Getting Minibar Products
                var RoomServiceProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Oda Servisi").ToList();

                //Products to be removed from Reservation
                var AddedRoomServiceProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Oda Servisi").ToList();
                ViewBag.AddedRoomServiceProducts = AddedRoomServiceProducts;


                return View(RoomServiceProducts);

            }

            return View();
        }


        [HttpPost]
        public ActionResult RoomServiceAddProduct(ServicePurchase servicePurchase, String ReservationID)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var CurrentReservationID = ReservationID;
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == CurrentReservationID.ToString()).ToList();
                TempData["SelectedReservationID"] = ReservationID;

                //Main Query for Getting Minibar Products
                var RoomServiceProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Oda Servisi").ToList();

                //Products to be removed from Reservation
                var AddedRoomServiceProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Oda Servisi").ToList();
                ViewBag.AddedRoomServiceProducts = AddedRoomServiceProducts;

                //Add ServicePurchase Item
                var itemTotalPrice = servicePurchase.ProductPrice * servicePurchase.Quantity;
                servicePurchase.TotalPrice = itemTotalPrice;

                var PurchaseDate = DateTime.Now;
                servicePurchase.PurchaseDate = PurchaseDate;

                db.ServicePurchase.Add(servicePurchase);
                db.SaveChanges();
               

                return RedirectToAction("RoomServiceAddProduct", "Admin");

            }

            return View();
        }


        public ActionResult DeleteRoomServiceProducts(IEnumerable<int> ServicePurchaseID, String ReservationID)
        {
            ProjectContext db = new ProjectContext();

            if (ServicePurchaseID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir kayıt seçmelisiniz !.";
            }

            else
            {

                foreach (var item in ServicePurchaseID)
                {

                    var delete = db.ServicePurchase.FirstOrDefault(x => x.ServicePurchaseID == item);
                    TempData["SelectedReservationID"] = ReservationID;

                    if (delete != null)
                    {
                        db.ServicePurchase.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }

            return RedirectToAction("RoomServiceAddProduct", "Admin");
        }
        //Room Service Controllers





        //RestaurantCafe  Controllers
        public ActionResult RestaurantCafe(String RoomNumber)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Main Query for Getting Reservation
                var Today = DateTime.Today;
                var Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && Today >= x.CheckIn && Today <= x.CheckOut && x.RoomNumber == RoomNumber && x.ReservationStatus != "Cancelled").ToList();

                //Update Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];

                if (RoomNumber != null && !Reservation.Any())
                {
                    ViewBag.NoRoom = "Seçmiş olduğunuz odada rezervasyon bulunmamaktadır";
                }



                return View(Reservation);

            }

            return View();
        }


        public ActionResult RestaurantCafeGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyona oda servisi ekleme işlemi için bir kayıt seçmelisiniz!";
                return RedirectToAction("RestaurantCafe", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;
                return RedirectToAction("RestaurantCafeAddProduct", "Admin");
            }

        }


        public ActionResult RestaurantCafeAddProduct()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData["SelectedReservationID"];
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString()).ToList();

                //Main Query for Getting Minibar Products
                var RestaurantCafeProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Resturant / Cafe").ToList();

                //Products to be removed from Reservation
                var AddedRestaurantCafeProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Resturant / Cafe").ToList();
                ViewBag.AddedRestaurantCafeProducts = AddedRestaurantCafeProducts;


                return View(RestaurantCafeProducts);

            }

            return View();
        }


        [HttpPost]
        public ActionResult RestaurantCafeAddProduct(ServicePurchase servicePurchase, String ReservationID)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var CurrentReservationID = ReservationID;
                ViewBag.Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == CurrentReservationID.ToString()).ToList();
                TempData["SelectedReservationID"] = ReservationID;

                //Main Query for Getting Minibar Products
                var RestaurantCafeProducts = db.Service.Where(x => x.AccountID.ToString() == AccountID && x.ServiceType == "Resturant / Cafe").ToList();

                //Products to be removed from Reservation
                var AddedRestaurantCafeProducts = db.ServicePurchase.Where(x => x.AccountID.ToString() == AccountID && x.ReservationID.ToString() == ReservationID.ToString() && x.ServiceType == "Resturant / Cafe").ToList();
                ViewBag.AddedRestaurantCafeProducts = AddedRestaurantCafeProducts;

                //Add ServicePurchase Item
                var itemTotalPrice = servicePurchase.ProductPrice * servicePurchase.Quantity;
                servicePurchase.TotalPrice = itemTotalPrice;

                var PurchaseDate = DateTime.Now;
                servicePurchase.PurchaseDate = PurchaseDate;

                db.ServicePurchase.Add(servicePurchase);
                db.SaveChanges();


                return RedirectToAction("RestaurantCafeAddProduct", "Admin");

            }

            return View();
        }


        public ActionResult DeleteRestaurantCafeProducts(IEnumerable<int> ServicePurchaseID, String ReservationID)
        {
            ProjectContext db = new ProjectContext();

            if (ServicePurchaseID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir kayıt seçmelisiniz !.";
            }

            else
            {

                foreach (var item in ServicePurchaseID)
                {

                    var delete = db.ServicePurchase.FirstOrDefault(x => x.ServicePurchaseID == item);
                    TempData["SelectedReservationID"] = ReservationID;

                    if (delete != null)
                    {
                        db.ServicePurchase.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }

            return RedirectToAction("RoomServiceAddProduct", "Admin");
        }
        //RestaurantCafe Controllers




        //KBS CheckIn Controllers
        public ActionResult KbsGuestCheckIn()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Main Query for Getting Reservation
                var Today = DateTime.Today;
                var Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.CheckIn == Today && x.ReservationStatus != "Cancelled").ToList();

                //Send Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];


                return View(Reservation);

            }

            return View();
        }


        public ActionResult KbsGuestCheckInGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyonu KBS'ye göndermek için bir kayıt seçmelisiniz!";
                return RedirectToAction("KbsGuestCheckIn", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;
                return RedirectToAction("KbsGuestCheckInSend", "Admin");
            }

        }

        public ActionResult KbsGuestCheckInSend()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData.Peek("SelectedReservationID");
                var Reservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID.ToString());

                var FirstName = Reservation.FirstName;
                ViewBag.FirstName = FirstName;
                var LastName = Reservation.LastName;
                ViewBag.LastName = LastName;
                var CheckIn = Reservation.CheckIn?.ToString("yyyy-MM-dd");
                ViewBag.CheckIn = CheckIn;
                var CheckOut = Reservation.CheckOut?.ToString("yyyy-MM-dd");
                ViewBag.CheckOut = CheckOut;
                var MobileNumber = Reservation.MobileNumber;
                ViewBag.MobileNumber = MobileNumber;
                var RoomNumber = Reservation.RoomNumber;
                ViewBag.RoomNumber = RoomNumber;

                //Kbs WebService Connection
                KbsService.SrvShsYtkTmlClient client = new SrvShsYtkTmlClient();
                //KBS User Credentials
                var KbsUser = db.Kbs.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                //Country Codes
                var Countries = client.ParametreListele(KbsUser.KlnTC, KbsUser.TssKod, KbsUser.KlnSifre,
                snfEnumServisUstKodParametreler.ULKELER);              
                ViewBag.Countries = Countries.Items.ToList();
                //CountryCodes Result
                var RequestResult = Countries.sonuc;
                ViewBag.Sonuc = RequestResult.Basarili;
                ViewBag.Mesaj = RequestResult.Mesaj;
                ViewBag.Hata = RequestResult.HataKodu;



                return View();

            }

            return View();
        }

        [HttpPost]
        public ActionResult KbsGuestCheckInSend(Kbs kbs, MusteriKimlikNoGirisTalep MusteriBilgi)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData.Peek("SelectedReservationID");
                var Reservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID.ToString());

                var FirstName = Reservation.FirstName;
                ViewBag.FirstName = FirstName;
                var LastName = Reservation.LastName;
                ViewBag.LastName = LastName;
                var CheckIn = Reservation.CheckIn?.ToString("yyyy-MM-dd");
                ViewBag.CheckIn = CheckIn;
                var CheckOut = Reservation.CheckOut?.ToString("yyyy-MM-dd");
                ViewBag.CheckOut = CheckOut;
                var MobileNumber = Reservation.MobileNumber;
                ViewBag.MobileNumber = MobileNumber;
                var RoomNumber = Reservation.RoomNumber;
                ViewBag.RoomNumber = RoomNumber;


                //Kbs WebService Connection
                KbsService.SrvShsYtkTmlClient client = new SrvShsYtkTmlClient();
                //KBS User Credentials
                var KbsUser = db.Kbs.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                //Calling Web Service Method
                client.MusteriKimlikNoGiris(KbsUser.KlnTC, KbsUser.TssKod, KbsUser.KlnSifre, MusteriBilgi);
                //Country Codes
                var Countries = client.ParametreListele(KbsUser.KlnTC, KbsUser.TssKod, KbsUser.KlnSifre,
                snfEnumServisUstKodParametreler.ULKELER);
                ViewBag.Countries = Countries.Items.ToList();
                //Request Result
                KbsService.Sonuc RequestResult = new KbsService.Sonuc();               
                ViewBag.Sonuc = RequestResult.Basarili;
                ViewBag.Mesaj = RequestResult.Mesaj;
                ViewBag.Hata = RequestResult.HataKodu;

                client.Close();


                return View();
            }

            return RedirectToAction("KbsGuestCheckInSend", "Admin");
        }
        //KBS CheckIn Controllers




        //KBS CheckOut Controllers
        public ActionResult KbsGuestCheckOut()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Main Query for Getting Reservation
                var Today = DateTime.Today;
                var Reservation = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.CheckOut == Today && x.ReservationStatus != "Cancelled").ToList();

                //Send Record Selection Validation
                ViewBag.ReservationNotSelected = TempData["ReservationNotSelected"];


                return View(Reservation);

            }

            return View();
        }


        public ActionResult KbsGuestCheckOutGetSelected(String ReservationID)
        {

            ProjectContext db = new ProjectContext();

            //Reservation ID Control for Updating Record
            if (ReservationID == null)
            {
                TempData["ReservationNotSelected"] = "Rezervasyonu KBS'ye göndermek için bir kayıt seçmelisiniz!";
                return RedirectToAction("KbsGuestCheckIn", "Admin");
            }

            else
            {
                var SelectedReservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID);
                var SelectedReservationID = SelectedReservation.ReservationID;
                TempData["SelectedReservationID"] = SelectedReservationID;
                return RedirectToAction("KbsGuestCheckInSend", "Admin");
            }

        }

        public ActionResult KbsGuestCheckOutSend()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Reservation
                var ReservationID = TempData["SelectedReservationID"];
                var Reservation = db.Reservation.FirstOrDefault(x => x.ReservationID.ToString() == ReservationID.ToString());

                var FirstName = Reservation.FirstName;
                ViewBag.FirstName = FirstName;
                var LastName = Reservation.LastName;
                ViewBag.LastName = LastName;
                var CheckIn = Reservation.CheckIn?.ToString("yyyy-MM-dd");
                ViewBag.CheckIn = CheckIn;
                var CheckOut = Reservation.CheckOut?.ToString("yyyy-MM-dd");
                ViewBag.CheckOut = CheckOut;
                var MobileNumber = Reservation.MobileNumber;
                ViewBag.MobileNumber = MobileNumber;
                var RoomNumber = Reservation.RoomNumber;
                ViewBag.RoomNumber = RoomNumber;



                return View(Reservation);

            }

            return View();
        }

        [HttpPost]
        public ActionResult KbsGuestCheckOutSend(Kbs kbs, MusteriKimlikNoCikisTalep MusteriBilgi)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;


                //Kbs WebService Connection
                KbsService.SrvShsYtkTmlClient client = new SrvShsYtkTmlClient();

                //KBS User Credentials
                var KbsUser = db.Kbs.FirstOrDefault(x => x.AccountID.ToString() == AccountID);

                //Calling Web Service Method
                client.MusteriKimlikNoCikis(KbsUser.KlnTC, KbsUser.TssKod, KbsUser.KlnSifre, MusteriBilgi);

                //Request Result
                KbsService.Sonuc RequestResult = new KbsService.Sonuc();


                ViewBag.Sonuc = RequestResult.Basarili;
                ViewBag.Mesaj = RequestResult.Mesaj;
                ViewBag.Hata = RequestResult.HataKodu;

                client.Close();


                return View();
            }

            return View();
        }
        //KBS CheckIn Controllers


        //Export Controllers
        public ActionResult Export()
        {

            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                 return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];
               
                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID); 
                ViewBag.CompanyName = user.CompanyName;
                ViewBag.EmailAddress = user.EmailAddress;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Export Reservations Main Query
                var Reservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID).OrderByDescending(s => s.CheckIn).ToList();

                //Cancelled Reservations Bathh
                ViewBag.Badge = "danger";
                ViewBag.Text = "İptal Edildi";

                return View(Reservations);
            
            }


            return View();

        }

        public IEnumerable<Reservation> ExportGetReservations()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            //Yes userId is found. Mission accomplished.
            string AccountID = OtorezCookie.Values["AccountID"];

            //DB Connection
            ProjectContext db = new ProjectContext();

            //Prepare All Reservations to Export Main Query
            var Reservations = db.Reservation.Where(x => x.AccountID.ToString() == AccountID).ToList();

            return Reservations;
        }

        public ActionResult ExportToExcel()
        {
            //Creating Grid View
            var gv = new GridView();
            gv.DataSource = this.ExportGetReservations();
            gv.DataBind();

            //Response Paramaters
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Report" + "-" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + ".xls");
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

            return View("Export", "Admin");

        }
        //Export Controllers





        //Account Settings Controllers
        public ActionResult AccountSettings()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notifications
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Account Validations
                ViewBag.AccountInfoUpdated = TempData["AccountInfoUpdated"];
                ViewBag.WrongPassword = TempData["WrongPassword"];
                ViewBag.ConfirmPasswordError = TempData["ConfirmPasswordError"];
                ViewBag.CurrentPasswordError = TempData["CurrentPasswordError"];

                //Update Account Main Query
                var UpdateUser = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);

                return View(UpdateUser);
            }

            return View();
        }


        [HttpPost]
        public ActionResult AccountSettings(Account account)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalizations
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notifications
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Update Account Main Query
                var UpdateUser = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);

                //Account Info Update
                if (account.Password == UpdateUser.Password)
                {


                    if (account.CompanyName != null)

                    {
                        UpdateUser.CompanyName = account.CompanyName;
                        db.SaveChanges();
                    }

                    TempData["AccountInfoUpdated"] = "Hesap bilgileriniz güncellenmiştir!";

                    return RedirectToAction("AccountSettings", "Admin");

                }

                else

                {
                    TempData["WrongPassword"] = "Girmiş olduğunuz şifre hatalıdır! Lütfen tekrar deneyiniz";

                    return RedirectToAction("AccountSettings", "Admin");

                }

            }

            return View();
        }


        [HttpPost]
        public ActionResult AccountChangePassword(Account account, String NewPassword, String ConfirmNewPassword)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notifications
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Update Account Main Query
                var UpdateUser = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);


                //Account Password Update
                if (account.Password == UpdateUser.Password)
                {
                    if (NewPassword == ConfirmNewPassword)
                    {

                        UpdateUser.Password = NewPassword;

                        db.SaveChanges();

                        TempData["AccountPasswordUpdated"] = "Hesap şifreniz güncellenmiştir! Lütfen tekrar giriş yapınız.";

                        OtorezCookie.Expires = DateTime.Now.AddSeconds(0);
                        Response.Cookies.Add(OtorezCookie);

                        return RedirectToAction("Login", "Admin");

                    }

                    else
                    {

                        TempData["ConfirmPasswordError"] = "Yeni şifrenizi kontrol ederek tekrar giriniz!";

                        return RedirectToAction("AccountSettings", "Admin");

                    }
                }

                else
                {
                    TempData["CurrentPasswordError"] = "Mevcut şifrenizi kontrol ederek tekrar giriniz!";

                    return RedirectToAction("AccountSettings", "Admin");
                }

            }


            return View();


        }
        //Account Settings Controllers





        //Hotel Settings Controllers
        public ActionResult HotelSettings()
        {
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Count of Each Rooms
                var Single = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Single Room").ToList();
                if (Single != null)
                {
                    var SingleQuantity = Single.Sum(x => x.RoomCount);
                    TempData["SingleQuantity"] = SingleQuantity;
                }
                var Double = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Double Room").ToList();
                if (Double != null)
                {
                    var DoubleQuantity = Double.Sum(x => x.RoomCount);
                    TempData["DoubleQuantity"] = DoubleQuantity;
                }
                var Twin = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Twin Room").ToList();
                if (Twin != null)
                {
                    var TwinQuantity = Twin.Sum(x => x.RoomCount);
                    TempData["TwinQuantity"] = TwinQuantity;
                }
                var Triple = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Triple Room").ToList();
                if (Triple != null)
                {
                    var TripleQuantity = Triple.Sum(x => x.RoomCount);
                    TempData["TripleQuantity"] = TripleQuantity;
                }
                var Quad = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Quad Room").ToList();
                if (Quad != null)
                {
                    var QuadQuantity = Quad.Sum(x => x.RoomCount);
                    TempData["QuadQuantity"] = QuadQuantity;
                }
                var Family = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Family Room").ToList();
                if (Family != null)
                {
                    var FamilyQuantity = Family.Sum(x => x.RoomCount);
                    TempData["FamilyQuantity"] = FamilyQuantity;
                }
                var Suit = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Suit Room").ToList();
                if (Suit != null)
                {
                    var SuitQuantity = Suit.Sum(x => x.RoomCount);
                    TempData["SuitQuantity"] = SuitQuantity;
                }
                var Bungalow = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Bungalow").ToList();
                if (Bungalow != null)
                {
                    var BungalowQuantity = Bungalow.Sum(x => x.RoomCount);
                    TempData["BungalowQuantity"] = BungalowQuantity;
                }
                var Villa = db.Room.Where(x => x.AccountID == user.AccountID && x.RoomType == "Villa").ToList();
                if (Villa != null)
                {
                    var VillaQuantity = Villa.Sum(x => x.RoomCount);
                    TempData["VillaQuantity"] = VillaQuantity;
                }

                var FinalSingleQuantity = TempData["SingleQuantity"];
                var FinalDoubleQuantity = TempData["DoubleQuantity"];
                var FinalTwinQuantity = TempData["TwinQuantity"];
                var FinalTripleQuantity = TempData["TripleQuantity"];
                var FinalQuadQuantity = TempData["QuadQuantity"];
                var FinalFamilyQuantity = TempData["FamilyQuantity"];
                var FinalSuitQuantity = TempData["SuitQuantity"];
                var FinalBungalowQuantity = TempData["BungalowQuantity"];
                var FinalVillaQuantity = TempData["VillaQuantity"];

                var StaticTotalRoomCount = Convert.ToInt32(FinalSingleQuantity) + Convert.ToInt32(FinalDoubleQuantity) + Convert.ToInt32(FinalTwinQuantity) + Convert.ToInt32(FinalTripleQuantity) + Convert.ToInt32(FinalQuadQuantity) + Convert.ToInt32(FinalFamilyQuantity) + Convert.ToInt32(FinalSuitQuantity) + Convert.ToInt32(FinalBungalowQuantity) + Convert.ToInt32(FinalVillaQuantity);
                ViewBag.StaticTotalRoomCount = StaticTotalRoomCount;
                TempData["StaticTotalRoomCount"] = StaticTotalRoomCount;
                //Count of Each Rooms

                //Hotel Settings Main Query
                var Rooms = db.Room.Where(x => x.AccountID.ToString() == AccountID).ToList();

                //MemberShip Status Room Limits 
                var MemberShipStatus = user.MembershipStatus;

                if (MemberShipStatus == "Bronze")
                {
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 10 oda ekleyebilirsiniz.";
                }

                if (MemberShipStatus == "Silver")
                {
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 20 oda ekleyebilirsiniz.";
                }

                if (MemberShipStatus == "Gold")
                {
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 30 oda ekleyebilirsiniz.";
                }

                //Room Update Validations       
                ViewBag.DublicateRoomNumber = TempData["DublicateRoomNumber"];
                ViewBag.NoDelete = TempData["NoDelete"];
                ViewBag.Membership = TempData["Membership"];

                return View(Rooms);

            }

            return View();
        }


        [HttpPost]
        public ActionResult HotelSettings(int? RoomCount, Room room)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Add New Room Type Validation
                var ExistingRoomType = db.Room.FirstOrDefault(x => x.AccountID.ToString() == AccountID && x.RoomType == room.RoomType);

                //Membership Room Limit Validation
                var Membership = user.MembershipStatus;
                var StaticTotalRoomCount = TempData["StaticTotalRoomCount"];


                int MembershipLimit = 0;

                if (user.MembershipStatus == "Bronze")
                {
                    MembershipLimit = 10;
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 10 oda ekleyebilirsiniz.";
                }

                if (user.MembershipStatus == "Silver")
                {
                    MembershipLimit = 20;
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 20 oda ekleyebilirsiniz.";
                }

                if (user.MembershipStatus == "Gold")
                {
                    MembershipLimit = 30;
                    ViewBag.MemberShipStatus = "Mevcut üyelik paketiniz ile toplam 30 oda ekleyebilirsiniz.";
                }

                //Add New Rooms
                if (room.RoomType != null && RoomCount != null && room.RoomNumber != null)
                {

                    var DublicateRoomNumber = db.Room.FirstOrDefault(x => x.AccountID.ToString() == AccountID && x.RoomNumber == room.RoomNumber);

                    if (Convert.ToInt32(StaticTotalRoomCount) + RoomCount <= MembershipLimit && DublicateRoomNumber == null)
                    {
                        db.Room.Add(room);
                        db.SaveChanges();
                    }

                    else if (DublicateRoomNumber != null)
                    {
                        TempData["DublicateRoomNumber"] = "Bu oda kapı numarası ile başka bir oda kayıtlıdır.";
                    }

                    else
                    {

                        TempData["Membership"] = "Mevcut üyelik paketiniz ile maksimum" + " " + MembershipLimit + " " + "oda ekleyebilirsiniz";
                    }


                }


                return RedirectToAction("HotelSettings", "Admin");

            }

            return View();
        }


        public ActionResult HotelSettingsDeleteRoom(IEnumerable<int> RoomID)
        {
            ProjectContext db = new ProjectContext();

            if (RoomID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir kayıt seçmelisiniz !.";
            }

            else
            {

                foreach (var item in RoomID)
                {

                    var delete = db.Room.FirstOrDefault(x => x.RoomID == item);

                    if (delete != null)
                    {
                        db.Room.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }

         

            return RedirectToAction("HotelSettings", "Admin");
        }
        //Hotel Settings Controllers





        //Service Settings Controllers 
        public ActionResult ServiceSettings()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];
    
            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Service Main Query
                var Service = db.Service.Where(x => x.AccountID.ToString() == AccountID);

                //Service Delete Validation
                ViewBag.NoDelete = TempData["NoDelete"];

                return View(Service);

            }

            return View();
        }


        [HttpPost]
        public ActionResult ServiceSettings(Service service)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                var DublicateProduct = db.Service.FirstOrDefault(x => x.AccountID.ToString() == AccountID && x.ServiceType == service.ServiceType && x.ProductName == service.ProductName);

                if (DublicateProduct == null)
                {
                    db.Service.Add(service);
                    db.SaveChanges();
                }
              
                else
                {
                    ViewBag.DublicateProduct = "Bu isimle başka bir ürün kayıtlıdır.";
                }
            
                //Service Main Query
                var Service = db.Service.Where(x => x.AccountID.ToString() == AccountID);

                return View(Service);

            }

            return View();
        }


        public ActionResult ServiceSettingsDeleteProduct(IEnumerable<int> ServiceID)
        {
            ProjectContext db = new ProjectContext();

            if (ServiceID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir ürün seçmelisiniz!";
            }

            else
            {

                foreach (var item in ServiceID)
                {

                    var delete = db.Service.FirstOrDefault(x => x.ServiceID == item);

                    if (delete != null)
                    {
                        db.Service.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }

        
            return RedirectToAction("ServiceSettings", "Admin");
        }





        //KBS Settings Controllers 
        public ActionResult KbsSettings()
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

                //Kbs Main Query
                var Kbs = db.Kbs.Where(x => x.AccountID.ToString() == AccountID);

                //Service Delete Validation
                ViewBag.NoDelete = TempData["NoDelete"];

                return View(Kbs);

            }

            return View();
        }



        [HttpPost]
        public ActionResult KbsSettings(Kbs kbs)
        {
            //Request Cookie
            HttpCookie OtorezCookie = Request.Cookies["OtorezCookie"];

            if (OtorezCookie == null)
            {
                return RedirectToAction("Login", "Admin");
            }


            if (!string.IsNullOrEmpty(OtorezCookie.Values["AccountID"]))

            {
                //Yes userId is found. Mission accomplished.
                string AccountID = OtorezCookie.Values["AccountID"];

                //DB Connection
                ProjectContext db = new ProjectContext();

                //User Personalization
                var user = db.Account.FirstOrDefault(x => x.AccountID.ToString() == AccountID);
                ViewBag.AccountID = user.AccountID;
                ViewBag.EmailAddress = user.EmailAddress;
                ViewBag.CompanyName = user.CompanyName;

                //Pending Payment Notification
                var PendingPayment = db.Reservation.Where(x => x.AccountID.ToString() == AccountID && x.PaymentStatus != "Tamamlandı" && x.ReservationStatus != "Cancelled").ToList();
                var PendingPaymentCount = PendingPayment.Count();
                ViewBag.PendingPaymentCount = PendingPaymentCount;

           
                db.Kbs.Add(kbs);
                db.SaveChanges();
                           
                //Kbs Main Query
                var Kbs = db.Kbs.Where(x => x.AccountID.ToString() == AccountID);

                return View(Kbs);

            }

            return View();
        }


        public ActionResult KbsSettingsDeleteUser(IEnumerable<int> KbsID)
        {
            ProjectContext db = new ProjectContext();

            if (KbsID == null)
            {
                TempData["NoDelete"] = "Silme işlemi için en az bir ürün seçmelisiniz!";
            }

            else
            {

                foreach (var item in KbsID)
                {

                    var delete = db.Kbs.FirstOrDefault(x => x.KbsID == item);

                    if (delete != null)
                    {
                        db.Kbs.Remove(delete);
                        db.SaveChanges();
                    }

                }

            }


            return RedirectToAction("KbsSettings", "Admin");
        }
        //KBS Settings Controllers 




    }

}