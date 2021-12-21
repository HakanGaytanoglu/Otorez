using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.AdminModels
{
    [Table("Kbs")]
    public class Kbs
    {
        [Key]
        [Column(Order = 1)]
        public int KbsID { get; set; }

        [Column(Order = 2)]
        public long KlnTC { get; set; }

        public long TssKod { get; set; }

        public string KlnSifre { get; set; }

        public int? AccountID { get; set; }

    }
}