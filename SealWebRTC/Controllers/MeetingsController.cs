using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SealWebRTC.Helper;
using SealWebRTC.Models;

namespace SealWebRTC.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(EFContext context, ILogger<MeetingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //// GET: Meetings
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Meetings
        //    .Include(m => m.TypeAttention)
        //            .Include(m => m.UserClient)
        //            .Include(m => m.UserManager)
        //            //.Where(m => m.Type == 1)
        //            .ToListAsync());
        //}

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IndexData(DataTableRequest request)
        {
            dynamic datatable = new JObject();
            datatable.draw = request.Draw;

            IQueryable<Meeting> count = _context.Meetings
                .Include(m => m.TypeAttention)
                .Include(m => m.UserClient)
                .Include(m => m.UserManager)
                .OrderByDescending(m => m.Created);

            if (!String.IsNullOrEmpty(request.Columns[0].Search.Value))
            {
                String[] minMax = (request.Columns[0].Search.Value).Split("-yadcf_delim-");
                DateTime min = new DateTime();
                DateTime max = new DateTime();
                //Buscar por BeginDate
                if (minMax[1] == "")
                {
                    min = DateTime.ParseExact(minMax[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    count = count.Where(m => m.Created >= min);
                }
                //Buscar por EndDate
                if (minMax[0] == "")
                {
                    max = DateTime.ParseExact(minMax[1], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    max = max.AddHours(23).AddMinutes(59).AddSeconds(59);
                    count = count.Where(m => m.Created <= max);
                }
                //Buscar entre BeginDate and EndDate
                if (minMax[0] != "" && minMax[1] != "")
                {
                    min = DateTime.ParseExact(minMax[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    max = DateTime.ParseExact(minMax[1], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    max = max.AddHours(23).AddMinutes(59).AddSeconds(59);
                    count = count.Where(m => m.Created >= min && m.Created <= max);
                }
            }
            if (!String.IsNullOrEmpty(request.Columns[1].Search.Value))
            {
                count = count.Where(m => (m.UserManager.FirstName + " " + m.UserManager.LastName).Contains(request.Columns[1].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[2].Search.Value))
            {
                count = count.Where(m => (m.UserClient.FirstName + " " + m.UserClient.LastName).Contains(request.Columns[2].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[3].Search.Value))
            {
                count = count.Where(m => m.TypeAttention.Name.Contains(request.Columns[3].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[4].Search.Value))
            {
                if ((request.Columns[4].Search.Value).ToString() == "Ticket")
                {
                    count = count.Where(m => m.Type == 1);
                }
                if ((request.Columns[4].Search.Value).ToString() == "Programada")
                {
                    count = count.Where(m => m.Type == 2);
                }
            }
            if (!String.IsNullOrEmpty(request.Columns[6].Search.Value))
            {
                if ((request.Columns[6].Search.Value).ToString() == "Programada")
                {
                    count = count.Where(m => m.Status == 0);
                }
                if ((request.Columns[6].Search.Value).ToString() == "Espera")
                {
                    count = count.Where(m => m.Status == 1);
                }
                if ((request.Columns[6].Search.Value).ToString() == "Atencion")
                {
                    count = count.Where(m => m.Status == 2);
                }
                if ((request.Columns[6].Search.Value).ToString() == "Cerrado")
                {
                    count = count.Where(m => m.Status == 3);
                }
                if ((request.Columns[6].Search.Value).ToString() == "Anulado")
                {
                    count = count.Where(m => m.Status == 4);
                }
            }

            datatable.recordsTotal = count.Count();
            int contFilter = count.Count();
            datatable.recordsFiltered = contFilter;
            var listFiltered = count.Skip(request.Start).Take(request.Length).ToList();

            datatable.data = new JArray() as dynamic;
            foreach (var item in listFiltered)
            {
                dynamic obj = new JObject();

                obj.Created = item.Created.ToString("yyyy-MM-dd");
                obj.UserManager = item.UserManager == null ? "" : item.UserManager.FirstName + " " + item.UserManager.LastName;
                obj.UserClient = @item.UserClient.FirstName + " " + @item.UserClient.LastName;
                obj.TypeAttention = item.TypeAttention.Name;
                var type = "";
                if (item.Type == 1)
                {
                    type = "Ticket";
                }
                if (item.Type == 2)
                {
                    type = "Programada";
                }
                obj.Type = type;
                var score = ((Convert.ToDouble(item.Score) * 100) / 5).ToString("0.00");
                obj.Score = score;
                var status = SealWebRTC.Helper.EnumExtensions.GetDisplayName((StatusMeeting)Enum.ToObject(typeof(StatusMeeting), Convert.ToInt32(item.Status)));
                obj.Status = status;
                obj.MeetId = item.Id;
                obj.MeetUniqueId = item.UniqueId;

                datatable.data.Add(obj);

            }
            return Content(datatable.ToString(), "application/json");
        }

        // GET: Meetings/Details/5
        public async Task<IActionResult> Details(String id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(m => m.UserManager)
                .Include(m => m.UserClient)
                .Include(m => m.TypeAttention)
                .FirstOrDefaultAsync(m => m.UniqueId == id);

            ViewBag.Messages = await _context.Messages.Where(m => m.MeetingId == meeting.Id).ToListAsync();

            ViewBag.Archives = await _context.Archives.Where(m => m.MeetingId == meeting.Id).ToListAsync();

            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // GET: Meetings/Create
        public IActionResult Create()
        {
            return View();
        }

        public async Task<ActionResult> CancelarCita(String UniqueId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();            
            return RedirectToAction("Salas");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UniqueId,MeetingDate,UserClient,UserManager,Created,Type")] Meeting meeting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(meeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound();
            }
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UniqueId,MeetingDate,UserClient,UserManager,Created,Type")] Meeting meeting)
        {
            if (id != meeting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meeting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex.Message);
                    if (!MeetingExists(meeting.Id))
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
            return View(meeting);
        }

        // GET: Meetings/Edit/5
        public async Task<IActionResult> EditStatus(String id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == id);
            if (meeting == null)
            {
                return NotFound();
            }
            return View(meeting);
        }

        // POST: Meetings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, [Bind("Id,UniqueId,Status")] Meeting meeting)
        {
            if (id != meeting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var meetingEdit = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == meeting.UniqueId);
                    meetingEdit.Status = meeting.Status;
                    _context.Update(meetingEdit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex.Message);
                    if (!MeetingExists(meeting.Id))
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
            return View(meeting);
        }

        // GET: Meetings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.Id == id);
        }
    }
}
