using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.SuperUserModels
{
    [Table("SuperUser")]
    public class SuperUser
    {
        [Key]
        [Column(Order = 1)]
        public int SuperUserID { get; set; }

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

    }
}