using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SealWebRTC.Helper;
using SealWebRTC.Models;

namespace SealWebRTC.Controllers
{
    public class UsersController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(EFContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.Where(m => m.Rol != 1).ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UniqueId,TypeDoc,NumberDoc,FirstName,LastName,Email,CellPhone,Password,Status,Created,Modified,LastAccess,ConfirmationEmail,Recovery,Rol,Enabled,Phone,Suministro,AccessKeyId,ChannelName,Region,SecretAccessKey")] User user)
        {
            if (ModelState.IsValid)
            {
                user.NumberDoc = user.NumberDoc.Trim();
                user.FirstName = user.FirstName.Trim();
                user.LastName = user.LastName.Trim();
                user.Email = user.Email.Trim().ToLower();
                user.UniqueId = Guid.NewGuid().ToString();
                user.Password = PasswordHash.GetMd5Hash(user.Password.Trim());

                user.Created = DateTime.Now;
                user.Modified = DateTime.Now;
                user.LastAccess = DateTime.Now;
                user.ConfirmationEmail = true;
                user.Recovery = false;
                user.Status = true;

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UniqueId,TypeDoc,NumberDoc,FirstName,LastName,Email,CellPhone,Password,Status,Created,Modified,LastAccess,ConfirmationEmail,Recovery,Rol,Enabled,Phone,Suministro,AccessKeyId,ChannelName,Region,SecretAccessKey")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                User userEdit = await _context.Users.FindAsync(user.Id);
                userEdit.TypeDoc = user.TypeDoc;
                userEdit.NumberDoc = user.NumberDoc.Trim();
                userEdit.FirstName = user.FirstName.Trim();
                userEdit.LastName = user.LastName.Trim();
                userEdit.Email = user.Email.Trim();
                userEdit.Phone = user.Phone != null ? user.Phone.Trim() : "";
                userEdit.Rol = user.Rol;
                userEdit.Suministro = user.Suministro != null ? user.Suministro : "";
                userEdit.Enabled = user.Enabled;
                userEdit.AccessKeyId = user.AccessKeyId != null ? user.AccessKeyId : "";
                userEdit.SecretAccessKey = user.SecretAccessKey != null ? user.SecretAccessKey : "";
                userEdit.ChannelName = user.ChannelName != null ? user.ChannelName : "";
                if (userEdit.Password != user.Password)
                {
                    userEdit.Password = PasswordHash.GetMd5Hash(user.Password);
                }
                _context.Update(userEdit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex.Message);
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            bool flag = true;

            var validacion1 = _context.Archives.Where(m => m.UserId == user.Id).ToList();
            if (validacion1.Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar porque existe una relacion con la tabla Archivos");
                flag = false;
            }

            var validacion2 = _context.Meetings.Where(m => m.UserClientId == user.Id).ToList();
            if (validacion2.Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar porque existe una relacion con la tabla Meetings como cliente");
                flag = false;
            }

            var validacion3 = _context.Meetings.Where(m => m.UserManagerId == user.Id).ToList();
            if (validacion3.Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar porque existe una relacion con la tabla Meetings como gestor");
                flag = false;
            }

            var validacion4 = _context.Messages.Where(m => m.UserId == user.Id).ToList();
            if (validacion4.Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar porque existe una relacion con la tabla Mensajes");
                flag = false;
            }

            if(flag)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(user);
            }
            
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
