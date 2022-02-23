using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SealWebRTC.Helper;
using SealWebRTC.Hubs;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class ClientController : Controller
    {
        private readonly EFContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly IHubContext<DashHub> _dashHub;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ClientController> _logger;
        

        public ClientController(EFContext context,
        IHubContext<TicketHub> hubContext,
        IHubContext<DashHub> dashHub,
        IHostEnvironment environment,
        IWebHostEnvironment env, ILogger<ClientController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _dashHub = dashHub;
            _hostEnvironment = environment;
            _env = env;
            _logger = logger;
        }

        [Route("MainClient")]
        public async Task<IActionResult> Index()
        {
            int userClientId = (int)HttpContext.Session.GetInt32("Id");

            DateTime fechaHoy = DateTime.Today;

            var Meetings = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m =>
                m.UserClientId == userClientId &&
                m.Type == 2 &&
                m.MeetingDateBegin >= fechaHoy &&
                //m.MeetingDateBegin.Year == DateTime.Now.Year &&
                //m.MeetingDateBegin.Month == DateTime.Now.Month &&
                //m.MeetingDateBegin.Day == DateTime.Now.Day &&
                m.Status != (int)StatusMeeting.Anulado && m.Status != (int)StatusMeeting.Cerrado)
                .ToListAsync();


            ViewBag.MeetingCount = await _context.Meetings
                .Where(m => m.Status == (int)StatusMeeting.Espera &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .CountAsync();
            
            return View(Meetings);
        }

        public async Task<IActionResult> GenerateTicket()
        {

            var configs = await _context.Configs.ToDictionaryAsync(m => m.Key, m => m.Value);

            var hoy = DateTime.Now;
            var hoyFecha = DateTime.ParseExact(hoy.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var feriados = await _context.Feriados.FirstOrDefaultAsync(m => m.Fecha == hoyFecha);

            if (feriados != null)
            {
                ModelState.AddModelError(string.Empty, "Estimado usuario el dia de hoy " + hoy.ToShortDateString() + " es feriado");
            }

            if (hoy.DayOfWeek == DayOfWeek.Sunday)
            {
                ModelState.AddModelError(string.Empty, "Estimado usuario el dia de hoy " + hoy.ToShortDateString() + " es Domingo");
            }

            if (configs.ContainsKey("Ticket hora Inicio"))
            {
                DateTime dt_init = DateTime.ParseExact(configs["Ticket hora Inicio"], "HH:mm", CultureInfo.InvariantCulture);
                TimeSpan time_init = dt_init - dt_init.Date;

                var currentTime = DateTime.Now - DateTime.Now.Date;

                if (time_init > currentTime)
                {
                    ViewBag.FueraHora = "El horario es de " + configs["Ticket hora Inicio"] + " a " + configs["Ticket hora fin"];
                    return View();
                }

                DateTime dt_end = DateTime.ParseExact(configs["Ticket hora fin"], "HH:mm", CultureInfo.InvariantCulture);
                TimeSpan time_end = dt_end - dt_end.Date;

                if (currentTime > time_end)
                {
                    ViewBag.FueraHora = "El horario es de " + configs["Ticket hora Inicio"] + " a " + configs["Ticket hora fin"];
                    return View();
                }


                int totalticket = Convert.ToInt32(configs["Ticket por dia"]);

                var quantity = await _context.Meetings.Where(m => m.MeetingDateBegin.Date == DateTime.Now.Date).CountAsync();

                if (quantity >= totalticket)
                {
                    ViewBag.FueraHora = "El total de ticket por día es de  " + configs["Ticket por dia"];
                    return View();
                }
            }



            int userClientId = (int)HttpContext.Session.GetInt32("Id");
            ViewBag.Attentions = await _context.Typeattentions.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Meeting = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.UserClientId == userClientId &&
                m.Status == (int)StatusMeeting.Espera &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .FirstOrDefaultAsync();


            ViewBag.MeetingCount = await _context.Meetings
                .Where(m => m.Status == (int)StatusMeeting.Espera &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .CountAsync();


            ViewBag.MeetingAtencion = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.UserClientId == userClientId &&
                m.Status == (int)StatusMeeting.Atencion &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .FirstOrDefaultAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTicket(int TypeAttentionId)
        {
            ModelState.AddModelError("", "");

            ViewBag.Attentions = await _context.Typeattentions.OrderBy(m => m.Name).ToListAsync();
            int userClientId = (int)HttpContext.Session.GetInt32("Id");

            var Meetings = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.UserClientId == userClientId &&
                m.Type == 1 &&
                m.Created.Year == DateTime.Now.Year &&
                m.Created.Month == DateTime.Now.Month &&
                m.Created.Day == DateTime.Now.Day &&
                m.Status == (int)StatusMeeting.Espera)
                .ToListAsync();

            var MeetingsCount = await _context.Meetings
                    .Include(m => m.TypeAttention)
                    .Where(m =>
                    m.Type == 1 &&
                    m.Created.Year == DateTime.Now.Year &&
                    m.Created.Month == DateTime.Now.Month &&
                    m.Created.Day == DateTime.Now.Day)
                    .ToListAsync();

            var hoy = DateTime.Now;
            var hoyFecha = DateTime.ParseExact(hoy.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var feriados = await _context.Feriados.FirstOrDefaultAsync(m => m.Fecha == hoyFecha);

            bool flag = true;

            

            if (hoy.DayOfWeek == DayOfWeek.Sunday)
            {
                ModelState.AddModelError(string.Empty, "Estimado usuario el dia de hoy " + hoy.ToShortDateString() + " es Domingo");
                flag = false;
            }

            if (feriados != null)
            {
                ModelState.AddModelError(string.Empty, "Estimado usuario el dia de hoy " + hoy.ToShortDateString() + " es feriado");
                flag = false;
            }

            if (flag)
            {
                if (Meetings.Count == 0)
                {
                    var listMeeting = await _context.Meetings
                           .Include(m => m.TypeAttention)
                           .Where(m => m.UserClientId == userClientId && m.Type == 1)
                           .ToListAsync();

                    foreach (var item in listMeeting)
                    {
                        item.Status = (int)StatusMeeting.Cerrado;
                        _context.Update(item);
                        await _context.SaveChangesAsync();
                    }

                    Meeting meeting = new Meeting();
                    meeting.UniqueId = Guid.NewGuid().ToString();
                    meeting.MeetingDateBegin = DateTime.Now;
                    meeting.MeetingDateEnd = DateTime.Now;
                    meeting.TypeAttentionId = TypeAttentionId;
                    meeting.UserClientId = (int)HttpContext.Session.GetInt32("Id");
                    meeting.Created = DateTime.Now;
                    meeting.Type = 1;
                    meeting.Number = MeetingsCount.Count + 1;
                    meeting.Status = (int)StatusMeeting.Espera;
                    _context.Add(meeting);
                    await _context.SaveChangesAsync();

                    //string jsonString = JsonSerializer.Serialize(meeting);

                    await _hubContext.Clients.All.SendAsync("AddTicket", "");
                }
                else
                {
                    ModelState.AddModelError("", "Usted ya tiene un ticket creado para hoy");
                }
            }


            ViewBag.Meeting = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.UserClientId == userClientId &&
                m.Status == 1 &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .FirstOrDefaultAsync();

            ViewBag.MeetingAtencion = await _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.UserClientId == userClientId &&
                m.Status == (int)StatusMeeting.Atencion &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .FirstOrDefaultAsync();

            await _dashHub.Clients.All.SendAsync("UpdateDash");

            ViewBag.MeetingCount = await _context.Meetings
                .Where(m => m.Status == (int)StatusMeeting.Espera &&
                m.MeetingDateBegin.Year == DateTime.Now.Year &&
                m.MeetingDateBegin.Month == DateTime.Now.Month &&
                m.MeetingDateBegin.Day == DateTime.Now.Day)
                .CountAsync();

            return View();
        }

        public async Task<IActionResult> ProgrammerMeeting()
        {
            ViewBag.Attentions = await _context.Typeattentions.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Meetings = await _context.Meetings
            .Where(m => m.MeetingDateBegin > DateTime.Now && m.Status != (int)StatusMeeting.Anulado)
            .ToListAsync();

            return View();
        }



        [HttpPost]
        public async Task<String> ProgrammerMeetingSave(String MeetingDateBegin, String MeetingDateEnd, int TypeAttentionId)
        {
            try
            {
                DateTime beginDate = DateTime.ParseExact(MeetingDateBegin, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(MeetingDateEnd, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                if (beginDate < DateTime.Now)
                {
                    return "No se puede programa un cita con una fecha pasada";
                }
                if (beginDate.Day == DateTime.Now.Day)
                {
                    return "No se puede programa un cita con una fecha pasada";
                }

                int count = await _context.Meetings.Where(m => m.MeetingDateBegin == beginDate && m.Status != (int)StatusMeeting.Anulado).CountAsync();

                if (count >= 2)
                {
                    return "Es un máximo de 2 citas en este horario";
                }

                int UserClientId = (int)HttpContext.Session.GetInt32("Id");
                count = await _context.Meetings.Where(m => m.UserClientId == UserClientId && m.Status == 0 && m.MeetingDateBegin > DateTime.Now).CountAsync();

                if (count > 1)
                {
                    return "Es un máximo de 2 citas programadas";
                }

                count = await _context.Meetings.Where(m =>
                m.MeetingDateBegin == beginDate &&
                m.MeetingDateEnd == endDate &&
                m.UserClientId == UserClientId &&
                m.Status == 0).CountAsync();

                if (count == 1)
                {
                    return "Ya tiene una cita para este día y hora";
                }

                var hoy = DateTime.Now;
                var hoyFecha = DateTime.ParseExact(hoy.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var feriados = await _context.Feriados.FirstOrDefaultAsync(m => m.Fecha == hoyFecha);

                if (hoy.DayOfWeek == DayOfWeek.Saturday)
                {
                    return "Este día es Sabado";
                }

                if (hoy.DayOfWeek == DayOfWeek.Sunday)
                {
                    return "Este día es Domingo";
                }

                var feriado = await _context.Feriados.Where(m => m.Fecha.Date == beginDate.Date).CountAsync();
                if (feriado > 0)
                {
                    return "Este día es feriado";
                }

                //UniqueId,MeetingDate,UserClient,UserManager,Created,Type")]
                Meeting meeting = new Meeting();
                meeting.UniqueId = Guid.NewGuid().ToString();
                meeting.MeetingDateBegin = beginDate;
                meeting.MeetingDateEnd = endDate;
                meeting.TypeAttentionId = TypeAttentionId;
                meeting.UserClientId = (int)HttpContext.Session.GetInt32("Id");
                meeting.Created = DateTime.Now;
                meeting.Type = 2;
                meeting.Status = (int)StatusMeeting.Programada;
                _context.Add(meeting);
                await _context.SaveChangesAsync();
                await _dashHub.Clients.All.SendAsync("UpdateDash");

                var usuarioEmail = await _context.Users.FirstOrDefaultAsync(m => m.Id == UserClientId);
                var atencion = await _context.Typeattentions.FirstOrDefaultAsync(m => m.Id == TypeAttentionId);

                //MODULO CORREO
                var path = _env.WebRootPath + "/plantilla_email/citaprogramada_email.html";
                String fileContents = System.IO.File.ReadAllText(path);
                fileContents = fileContents.Replace("$$CODEDATA1$$", usuarioEmail.FirstName.Trim() + " " + usuarioEmail.LastName.Trim());
                fileContents = fileContents.Replace("$$CODEDATA2$$", usuarioEmail.Email.Trim());
                if (!String.IsNullOrEmpty(usuarioEmail.Phone))
                {
                    fileContents = fileContents.Replace("$$CODEDATA3$$", usuarioEmail.Phone.Trim());
                }
                else
                {
                    fileContents = fileContents.Replace("$$CODEDATA3$$", "");
                }
                fileContents = fileContents.Replace("$$CODEDATA4$$", beginDate.ToString());
                fileContents = fileContents.Replace("$$CODEDATA5$$", atencion.Name);
                fileContents = fileContents.Replace("$$CODELINK$$", "https://videollamada.seal.com.pe/Login");

                String asunto = "SEAL CITA PROGRAMADA " + beginDate.ToString();

                SendEmailOutlook email = new SendEmailOutlook(usuarioEmail.Email, asunto, fileContents, _env.WebRootPath);
                if (!email.Send_Date())
                {
                    _logger.LogError(email.Error);
                }
                

                return "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return "ok";
        }


        [HttpPost]
        public async Task<String> GetCita()
        {
            try
            {

                int UserClientId = (int)HttpContext.Session.GetInt32("Id");
                var meetings = await _context.Meetings.Where(m => m.UserClientId == UserClientId && m.Status == 0).ToListAsync();
                foreach (var meeting in meetings)
                {
                    if (meeting != null)
                    {

                        var diff = DateTime.Now - meeting.MeetingDateBegin;
                        if (System.Math.Abs(diff.TotalMinutes) < 10)
                        {

                            HttpContext.Session.SetInt32("CurrentMeetingId", meeting.Id);
                            return "USTED TIENE UNA CITA PARA EL DÍA DE HOY A LAS " + meeting.MeetingDateBegin.ToString("HH:mm") + " HORAS";
                        }
                        else
                        {
                            //meeting.Status = (int)StatusMeeting.TicketAnulado;
                            //_context.Update(meeting);
                            //await _context.SaveChangesAsync();
                        }

                        //if (meeting.MeetingDateBegin.Year == DateTime.Now.Year &&
                        //    meeting.MeetingDateBegin.Month == DateTime.Now.Month &&
                        //    meeting.MeetingDateBegin.Day == DateTime.Now.Day)
                        //{
                        //    if (meeting.MeetingDateBegin > DateTime.Now.AddMinutes(-10) || meeting.MeetingDateBegin < DateTime.Now.AddMinutes(10))
                        //    {
                        //        HttpContext.Session.SetInt32("CurrentMeetingId", meeting.Id);
                        //        return "USTED TIENE UNA CITA PARA EL DÍA DE HOY A LAS " + meeting.MeetingDateBegin.ToString("HH:mm") + " HORAS";
                        //    }
                        //    else
                        //    {
                        //        meeting.Status = (int)StatusMeeting.TicketAnulado;
                        //        _context.Update(meeting);
                        //        await _context.SaveChangesAsync();
                        //    }
                        //}
                    }
                }
                return "fail";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return "fail";
        }

        public async Task<ActionResult> Score(Guid UniqueId)
        {

            ViewBag.UniqueId = UniqueId.ToString();
            return View();
        }



        [HttpPost]
        public async Task<ActionResult> Score(String UniqueId, int rate)
        {
            try
            {
                var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
                meeting.Status = (int)StatusMeeting.Cerrado;
                meeting.Score = rate;
                _context.Update(meeting);
                await _context.SaveChangesAsync();
                await _dashHub.Clients.All.SendAsync("UpdateDash");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return RedirectToAction("Index", "Client");
        }

        public async Task<ActionResult> CancelarCita(String UniqueId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            await _dashHub.Clients.All.SendAsync("UpdateDash");
            return RedirectToAction("GenerateTicket");

        }

        public async Task<ActionResult> CancelarCita2(String UniqueId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            //await _dashHub.Clients.All.SendAsync("UpdateDash");
            return RedirectToAction("Index");

        }

        public async Task<ActionResult> Meeting(String UniqueId, String PeerManager, String sessionId, String apiKey, String token)
        {
            var meeting = await _context.Meetings.Include(m => m.UserManager).FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            //meeting.UserManager = (int)HttpContext.Session.GetInt32("Id");
            //meeting.Status = (int)StatusMeeting.TicketAtencion;
            //meeting.DurationBegin = DateTime.Now;
            //_context.Update(meeting);
            //await _context.SaveChangesAsync();
            ViewBag.PeerManager = PeerManager;

            ViewBag.Messages = await _context.Messages.Where(m => m.MeetingId == meeting.Id).ToListAsync();
            ViewBag.UserManagerId = meeting.UserManagerId;

            User gestor = await _context.Users.FindAsync(meeting.UserManagerId);
            ViewBag.ChannelName = gestor.ChannelName;
            ViewBag.AccessKeyId = gestor.AccessKeyId;
            //ViewBag.SecretAccessKey = gestor.SecretAccessKey;

            ViewBag.ApiKey = apiKey;
            ViewBag.SessionId = sessionId;
            ViewBag.Token = token;

            return View(meeting);

            //return View();
        }

        public async Task<ActionResult> Perfil()
        {
            int UserId = (int)HttpContext.Session.GetInt32("Id");
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == UserId);
            return View(user);
        }

        public async Task<ActionResult> DocumentosPresentados()
        {
            int UserId = (int)HttpContext.Session.GetInt32("Id");
            var archivos = await _context.Archives.Where(m => m.UserId == UserId).ToListAsync();
            return View(archivos);
        }

        public async Task<ActionResult> HistorialAtenciones()
        {
            int UserId = (int)HttpContext.Session.GetInt32("Id");
            var meetings = await _context.Meetings.Include(m => m.TypeAttention).Where(m => m.UserClientId == UserId).ToListAsync();
            return View(meetings);
        }

        [HttpPost]
        public async Task<String> AddFile(IFormFile file, String UserUniqueId, String MeetingUniqueId)
        {
            var result = "OK";
            try
            {
                Archive archive = new Archive();
                archive.UniqueId = Guid.NewGuid().ToString();

                archive.Name = file.FileName;
                archive.Path = "/document/" + archive.UniqueId + Path.GetExtension(file.FileName);
                result = archive.Path;
                archive.UserId = (await _context.Users.FirstAsync(m => m.UniqueId == UserUniqueId)).Id;

                archive.MeetingId = (await _context.Meetings.FirstAsync(m => m.UniqueId == MeetingUniqueId)).Id;
                archive.Created = DateTime.Now;
                archive.Extension = Path.GetExtension(file.FileName);
                _context.Add(archive);
                await _context.SaveChangesAsync();
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/document", archive.UniqueId + Path.GetExtension(file.FileName));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                result = result + " -- " + ex.Message;
                _logger.LogError(ex.Message);
            }

            return result;
        }

        [HttpPost]
        public async Task<JsonResult> GetFiles(String UserUniqueId, String MeetingUniqueId)
        {

            var archives = _context.Archives
            .Include(m => m.User)
            .Include(m => m.Meeting)
            .Where(m => m.User.UniqueId == UserUniqueId && m.Meeting.UniqueId == MeetingUniqueId).ToList();

            var list = new List<GetArchives>();
            foreach (var item in archives)
            {
                var archive = new GetArchives();
                archive.Name = item.Name;
                archive.Path = item.Path;
                archive.Created = item.Created.ToString("yyyy-MM-dd HH:mm");
                list.Add(archive);
            }
            await _hubContext.Clients.All.SendAsync("GetFiles", UserUniqueId, MeetingUniqueId);
            return Json(list);
        }

        [HttpPost]
        public async Task<String> InitManagerMeeting(String UniqueId, String PeerManager)
        {
            await _hubContext.Clients.All.SendAsync("InitManagerMeeting", UniqueId, PeerManager);
            return "Ok";
        }

        public async Task<ActionResult> CitaIngresar()
        {
            int MeetingId = (int)HttpContext.Session.GetInt32("CurrentMeetingId");
            var meeting = await _context.Meetings.Where(m => m.Id == MeetingId).FirstOrDefaultAsync();
            meeting.Status = (int)StatusMeeting.Espera;
            _context.Update(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction("GenerateTicket");
        }
        public async Task<ActionResult> CitaAbandonar()
        {
            int MeetingId = (int)HttpContext.Session.GetInt32("CurrentMeetingId");
            var meeting = await _context.Meetings.Where(m => m.Id == MeetingId).FirstOrDefaultAsync();
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> CitaReprogramar()
        {
            int MeetingId = (int)HttpContext.Session.GetInt32("CurrentMeetingId");
            var meeting = await _context.Meetings.Where(m => m.Id == MeetingId).FirstOrDefaultAsync();
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction("ProgrammerMeeting");
        }

        public async Task<ActionResult> EsperandoCita(String UniqueId)
        {
            ViewBag.UniqueId = UniqueId.ToString();

            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            meeting.Status = (int)StatusMeeting.Espera;
            _context.Update(meeting);
            await _context.SaveChangesAsync();

            return View();
        }

        public ActionResult PaginaInvalidad()
        {
            return View();
        }

        public IActionResult ChangePassword() => View();


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(String password)
        {
            String ErrorMessage = "";
            if (!ValidatePassword(password, out ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
                return View();
            }


            var user = _context.Users.Find(HttpContext.Session.GetInt32("Id"));
            user.Password = PasswordHash.GetMd5Hash(password);
            user.Modified = DateTime.Now;
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Index", "Client");
        }

        private bool ValidatePassword(string password, out string ErrorMessage)
        {
            var input = password;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("La contraseña no debe estar vacía");
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(input))
            {
                ErrorMessage = "La contraseña debe contener al menos una letra minúscula.";
                return false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                ErrorMessage = "La contraseña debe contener al menos una letra mayúscula.";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(input))
            {
                ErrorMessage = "La contraseña no debe tener menos de 8 ni más de 15 caracteres.";
                return false;
            }
            else if (!hasNumber.IsMatch(input))
            {
                ErrorMessage = "La contraseña debe contener al menos un valor numérico.";
                return false;
            }

            else if (!hasSymbols.IsMatch(input))
            {
                ErrorMessage = "La contraseña debe contener al menos un carácter especial (!@#$%^&*()_+=\\[{\\]};:<>|./?,-).";
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
