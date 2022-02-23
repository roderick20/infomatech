
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class MenuViewComponent : ViewComponent
    {
        public IConfiguration _configuration { get; }
        private readonly EFContext _context;

        public MenuViewComponent(IConfiguration configuration, EFContext context)
        {
            _context = context;            
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {

            /*ViewBag.Menu = _context.Menus.ToList();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            var theme = "";
            if (HttpContext.Session.GetString("IsThemeDark") == "True")
            {
                theme = "dark";
            }
            ViewBag.Theme = theme;*/
            return View();
        }
    }
}
