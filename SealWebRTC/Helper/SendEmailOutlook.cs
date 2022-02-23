using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Net.Mime;

namespace SealWebRTC.Helper
{
    /*******************************************************************************************
* SendEmailOutlook
* Para en envió de correos
* Programador: Rodercik Cusirramos Montesinos
* Fecha de creacion: 22/06/2020
* Fecha de modificacion: 03/08/2020      
* *****************************************************************************************/

    public class SendEmailOutlook
    {
        public String ToEmail { get; set; }
        public String Subject { get; set; }
        public String Body { get; set; }

        public String WebRootPath { get; set; }

        public SendEmailOutlook(String ToEmail, String Subject, String Body, String WebRootPath)
        {
            this.ToEmail = ToEmail;
            this.Subject = Subject;
            this.Body = Body;
            this.WebRootPath = WebRootPath;
            Error = "";
        }

        public String Error { get; set; }

        public bool Send()
        {
            try
            {
                var fromAddress = new MailAddress("agile_projects@hotmail.com", "SEAL");
                var toAddress = new MailAddress(ToEmail);
                string subject = this.Subject;
                var smtp = new SmtpClient
                {
                    Host = "smtp-mail.outlook.com",
                    Port = 25,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential
                      ("agile_projects@hotmail.com", "Bk4cybertel?09")
                };



                String code2 = Guid.NewGuid().ToString();
                string html = this.Body.Replace("$$CODEIMAGEN$$", code2); ;
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);
                LinkedResource img = new LinkedResource(WebRootPath + "/plantilla_email/logo.jpg", MediaTypeNames.Image.Jpeg);
                img.ContentId = code2;
                htmlView.LinkedResources.Add(img);


                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject
                })
                {
                    message.AlternateViews.Add(htmlView);
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }



            return false;
        }
    }
}