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
    public class FeriadosController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<FeriadosController> _logger;


        public FeriadosController(EFContext context, ILogger<FeriadosController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ActionResult> Index()
        {
            return View(await _context.Feriados.ToListAsync());
        }

        // GET: FeriadosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FeriadosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(String fecha)
        {
            Feriado feriado = new Feriado();
            feriado.Fecha = DateTime.ParseExact(fecha, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            _context.Add(feriado);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Feriados");
        }



        // GET: FeriadosController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var feriado = await _context.Feriados.FindAsync(id);            
            _context.Feriados.Remove(feriado);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Feriados");
                       
        }

       
    }
}
