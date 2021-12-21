using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.AdminModels
{
    [Table("Account")]
    public class Account
    {
        [Key]
        [Column(Order = 1)]
        public int AccountID { get; set; }

        [Column(Order = 2)]
        [MaxLength(50)]
        public string EmailAddress { get; set; }

        [Column(Order = 3)]
        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string CompanyName { get; set; }

        public DateTime? AccountCreationDate { get; set; }

        public DateTime? DemoStartDate { get; set; }

        public DateTime? DemoEndDate { get; set; }

        public DateTime? AccountStartDate { get; set; }

        public DateTime? AccountEndDate { get; set; }

        [MaxLength(50)]
        public string AccountStatus { get; set; }

        [MaxLength(50)]
        public string MembershipStatus { get; set; }

        [MaxLength(50)]
        public string PaymentType { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; }

        public int? PaymentAmount { get; set; }

        [MaxLength(50)]
        public string Currency { get; set; }

        public virtual ICollection<Reservation> Reservation { get; set; }

        public virtual ICollection<Room> Room { get; set; }

        public virtual ICollection<Service> Service { get; set; }

        public virtual ICollection<ServicePurchase> ServicePurchase { get; set; }

        public virtual ICollection<Kbs> Kbs { get; set; }

    }
}