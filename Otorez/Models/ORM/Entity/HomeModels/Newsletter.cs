using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.HomeModels
{
    [Table("Newsletter")]
    public class Newsletter
    {
        [Key]
        [Column(Order = 1)]
        public int NewsletterID { get; set; }

        [Column(Order = 2)]
        [MaxLength(50)]
        public string LeadEmailAddress { get; set; }
    }
}