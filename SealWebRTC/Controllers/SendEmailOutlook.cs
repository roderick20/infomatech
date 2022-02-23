using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class SendEmailOutlook
    {
        public String ToEmail { get; set; }
        public String Subject { get; set; }
        public String Body { get; set; }
        public String WebRootPath { get; set; }
        public String OpcionPath { get; set; }

        

        public SendEmailOutlook(String ToEmail, String Subject, String Body, String WebRootPath)
        {
            this.ToEmail = ToEmail;
            this.Subject = Subject;
            this.Body = Body;
            this.WebRootPath = WebRootPath;
            Error = "";
            
        }

        public String Error { get; set; }

        public bool Send_Register()
        {
            try
            {
                //var smtp = new SmtpClient
                //{
                //    Host = "191.168.5.120",
                //    Port = 25,
                //    EnableSsl = false,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = false
                //};
                var fromAddress = new MailAddress("videollamadaseal@gmail.com", "SEAL");
                var toAddress = new MailAddress(ToEmail);
                string subject = this.Subject;
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("videollamadaseal@gmail.com", "Sealito?2021Viella"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                String guidHead = Guid.NewGuid().ToString();
                String guidFoot = Guid.NewGuid().ToString();
                String guidButton = Guid.NewGuid().ToString();

                string html = this.Body.Replace("$$CODEHEADER$$", guidHead);
                html = html.Replace("$$CODEFOOTER$$", guidFoot);
                html = html.Replace("$$CODEBUTTON$$", guidButton);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

                LinkedResource imgHead = new LinkedResource(WebRootPath + "/plantilla_email/header.jpeg", MediaTypeNames.Image.Jpeg);
                imgHead.ContentId = guidHead;
                htmlView.LinkedResources.Add(imgHead);

                LinkedResource imgFoot = new LinkedResource(WebRootPath + "/plantilla_email/footer.jpeg", MediaTypeNames.Image.Jpeg);
                imgFoot.ContentId = guidFoot;
                htmlView.LinkedResources.Add(imgFoot);

                LinkedResource imgButton = new LinkedResource(WebRootPath + "/plantilla_email/btnenter.jpeg", MediaTypeNames.Image.Jpeg);
                imgButton.ContentId = guidButton;
                htmlView.LinkedResources.Add(imgButton);


                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject
                })
                {
                    message.AlternateViews.Add(htmlView);
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return false;
        }

        public bool Send_Change_Password()
        {
            try
            {
                //var smtp = new SmtpClient
                //{
                //    Host = "191.168.5.120",
                //    Port = 25,
                //    EnableSsl = false,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = false
                //};
                var fromAddress = new MailAddress("videollamadaseal@gmail.com", "SEAL");
                var toAddress = new MailAddress(ToEmail);
                string subject = this.Subject;
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("videollamadaseal@gmail.com", "Sealito?2021Viella"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                String guidHead = Guid.NewGuid().ToString();
                String guidFoot = Guid.NewGuid().ToString();
                String guidButton = Guid.NewGuid().ToString();

                string html = this.Body.Replace("$$CODEHEADER$$", guidHead);
                html = html.Replace("$$CODEFOOTER$$", guidFoot);
                html = html.Replace("$$CODEBUTTON$$", guidButton);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

                LinkedResource imgHead = new LinkedResource(WebRootPath + "/plantilla_email/header.jpeg", MediaTypeNames.Image.Jpeg);
                imgHead.ContentId = guidHead;
                htmlView.LinkedResources.Add(imgHead);

                LinkedResource imgFoot = new LinkedResource(WebRootPath + "/plantilla_email/footer.jpeg", MediaTypeNames.Image.Jpeg);
                imgFoot.ContentId = guidFoot;
                htmlView.LinkedResources.Add(imgFoot);

                LinkedResource imgButton = new LinkedResource(WebRootPath + "/plantilla_email/btnpassword.jpeg", MediaTypeNames.Image.Jpeg);
                imgButton.ContentId = guidButton;
                htmlView.LinkedResources.Add(imgButton);


                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject
                })
                {
                    message.AlternateViews.Add(htmlView);
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return false;
        }

        public bool Send_Date()
        {
            try
            {
                //var smtp = new SmtpClient
                //{
                //    Host = "191.168.5.120",
                //    Port = 25,
                //    EnableSsl = false,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = false
                //};
                var fromAddress = new MailAddress("videollamadaseal@gmail.com", "SEAL");
                var toAddress = new MailAddress(ToEmail);
                string subject = this.Subject;
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("videollamadaseal@gmail.com", "Sealito?2021Viella"),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                String guidHead = Guid.NewGuid().ToString();
                String guidFoot = Guid.NewGuid().ToString();
                String guidButton = Guid.NewGuid().ToString();

                string html = this.Body.Replace("$$CODEHEADER$$", guidHead);
                html = html.Replace("$$CODEFOOTER$$", guidFoot);
                html = html.Replace("$$CODEBUTTON$$", guidButton);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);

                LinkedResource imgHead = new LinkedResource(WebRootPath + "/plantilla_email/header.jpeg", MediaTypeNames.Image.Jpeg);
                imgHead.ContentId = guidHead;
                htmlView.LinkedResources.Add(imgHead);

                LinkedResource imgFoot = new LinkedResource(WebRootPath + "/plantilla_email/footer.jpeg", MediaTypeNames.Image.Jpeg);
                imgFoot.ContentId = guidFoot;
                htmlView.LinkedResources.Add(imgFoot);

                LinkedResource imgButton = new LinkedResource(WebRootPath + "/plantilla_email/btnrecommend.jpeg", MediaTypeNames.Image.Jpeg);
                imgButton.ContentId = guidButton;
                htmlView.LinkedResources.Add(imgButton);


                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject
                })
                {
                    message.AlternateViews.Add(htmlView);
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return false;
        }
    }
}
