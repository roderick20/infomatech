using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class ConfigsController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<ConfigsController> _logger;

        public ConfigsController(EFContext context, ILogger<ConfigsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ActionResult> Index()
        {
            return View(await _context.Configs.ToListAsync());
        }


        // GET: ConfigsController/Edit/5
        public async Task<ActionResult> EditAsync(int id)
        {

            return View(await _context.Configs.FindAsync(id));
        }

        // POST: ConfigsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int id, String value)
        {
            var config = await _context.Configs.FindAsync(id);
            config.Value = value;
            _context.Update(config);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

       
    }
}
