using Otorez.Models.ORM.Entity.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Otorez.Models.ORM.Context.ProjectContext
{
    public class DataAccessHelper
    {
        readonly ProjectContext db = new ProjectContext();

        public List<Reservation> FetchReservations()
        {
            return db.Reservation.ToList();
        }

        public List<Room> FetchRequestsRooms()
        {
            return db.Room.ToList();
        }

        public List<Account> FetchAccounts()
        {
            return db.Account.ToList();
        }

        public int AddRequest(Reservation reservation)
        {
            db.Reservation.Add(reservation);
            db.SaveChanges();
            return reservation.ReservationID;
        }

        public int AddAccount(Account account)
        {
            db.Account.Add(account);
            db.SaveChanges();
            return account.AccountID;
        }
    }

}