using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SealWebRTC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class TestController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public TestController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public ActionResult Email()
        {
            var path = _env.WebRootPath + "/plantilla_email/confirmacion_email.html";
            String fileContents = System.IO.File.ReadAllText(path);
            fileContents = fileContents.Replace("$$UniqueId$$", "Link");

            SendEmailOutlook email = new SendEmailOutlook("roderick20@hotmail.com", "Seal confirmacion de correo", fileContents, _env.WebRootPath);
            email.Send_Register();

            return View();
        }

        
    }
}
