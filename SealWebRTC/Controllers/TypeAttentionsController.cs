using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SealWebRTC.Models;

namespace SealWebRTC.Controllers
{
    public class TypeAttentionsController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<TypeAttentionsController> _logger;

        public TypeAttentionsController(EFContext context, ILogger<TypeAttentionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: TypeAttentions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Typeattentions.ToListAsync());
        }

        // GET: TypeAttentions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeAttention = await _context.Typeattentions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeAttention == null)
            {
                return NotFound();
            }

            return View(typeAttention);
        }

        // GET: TypeAttentions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TypeAttentions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Typeattention typeAttention)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typeAttention);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typeAttention);
        }

        // GET: TypeAttentions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeAttention = await _context.Typeattentions.FindAsync(id);
            if (typeAttention == null)
            {
                return NotFound();
            }
            return View(typeAttention);
        }

        // POST: TypeAttentions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Typeattention typeAttention)
        {
            if (id != typeAttention.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typeAttention);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex.Message);
                    if (!TypeAttentionExists(typeAttention.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(typeAttention);
        }

        // GET: TypeAttentions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeAttention = await _context.Typeattentions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeAttention == null)
            {
                return NotFound();
            }

            return View(typeAttention);
        }

        // POST: TypeAttentions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var typeAttention = await _context.Typeattentions.FindAsync(id);
            _context.Typeattentions.Remove(typeAttention);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypeAttentionExists(int id)
        {
            return _context.Typeattentions.Any(e => e.Id == id);
        }
    }
}
