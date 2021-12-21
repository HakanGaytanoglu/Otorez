using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.HomeModels
{
    [Table("ContactForm")]
    public class ContactForm
    {
        [Key]
        [Column(Order = 1)]
        public int ContactFormID { get; set; }

        [Column(Order = 2)]
        [MaxLength(50)]
        [Required]
        public string EmailAddress { get; set; }

        [Column(Order = 3)]
        [MaxLength(50)]
        [Required]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string Subject { get; set; }
        
        public string Message { get; set; }

    }
}