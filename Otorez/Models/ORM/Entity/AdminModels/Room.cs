using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Entity.AdminModels
{
    [Table("Room")]
    public class Room
    {
        [Key]
        [Column(Order = 1)]
        public int RoomID { get; set; }

        [Column(Order = 2)]
        [MaxLength(50)]
        public string RoomType { get; set; }

        [MaxLength(50)]
        public string RoomNumber { get; set; }

        public int RoomCount { get; set; }

        public int? AccountID { get; set; }

    }
}