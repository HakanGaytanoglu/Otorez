using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.AdminModels
{
    [Table("Reservation")]
    public class Reservation

    {
        [Key]
        [Column(Order = 1)]
        public int ReservationID { get; set; }

        [Column(Order = 2)]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Column(Order = 3)]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Column(TypeName = "Date")]
        public DateTime? CheckIn { get; set; }

        [Column(TypeName = "Date")]
        public DateTime? CheckOut { get; set; }

        [MaxLength(50)]
        public string RoomType { get; set; }

        [MaxLength(50)]
        public string RoomNumber { get; set; }

        [MaxLength(50)]
        public string MobileNumber { get; set; }

        [MaxLength(50)]
        public string EmailAddress { get; set; }

        public int? TotalNumberAdults { get; set; }

        public int? TotalNumberKids { get; set; }

        public int? TotalNights { get; set; }
       
        [MaxLength(50)]
        public string BoardType { get; set; }

        [MaxLength(50)]
        public string PaymentType { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; }

        public int? PaymentAmount { get; set; }

        [MaxLength(50)]
        public string Currency { get; set; }

        [MaxLength(50)]
        public string Pets { get; set; }

        [MaxLength(50)]
        public string ReservationStatus { get; set; }

        public DateTime? ReservationDate { get; set; }

        public int? AccountID { get; set; }


    }
}