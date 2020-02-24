using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace qda
{
    namespace Net
    {
        public class SendMail
        {
            private static string password = Key.json["emailPassword"].ToString ();
            private static string address = Key.json["emailAddress"].ToString();

            public static string Send(string senderAddress = "", string receiverAddress = "", string subject = "", string body = "")
            {
                MailAddress sender = new MailAddress(senderAddress.Equals(string.Empty) ? address : senderAddress);
                MailAddress receiver = new MailAddress(receiverAddress.Equals(string.Empty) ? address : receiverAddress);

                SmtpClient smtp = null;
                MailMessage message = null;
                try
                {
                    smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(sender.Address, password),
                    };
                    message = new MailMessage(sender, receiver)
                    {
                        From = sender,
                        Subject = subject,
                        Body = body
                    };
                    smtp.Send(message);
                    return "Successfully sended";
                }
                catch (Exception e)
                {
                    return e.ToString();
                }
                finally
                {
                    smtp.Dispose();
                    message.Dispose();
                }
            }
        }
    }
}