using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using Otorez.Controllers;

namespace Otorez.Models.TriggerEmail.SuperUserEmails
{
    public class TriggerNewAccountMail
    {
        public static void SendAccountMail(string body, MailAddress toAddress)
        {
            var fromAddress = new MailAddress("helpdesk@otorez.com", "Otorez" + "'e Hoşgeldiniz");
            const string subject = "Otorez Kullanıcı Bilgileriniz";
            using (var smtp = new SmtpClient
            {
                Host = "mail.otorez.com",
                Port = 587,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, "Otorez@Help369")
                //trololol kısmı e-posta adresinin şifresi
            })

            {
                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                {
                    smtp.Send(message);
                }
            }

        }


    }
}