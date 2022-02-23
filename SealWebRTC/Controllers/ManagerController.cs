using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTokSDK;
using SealWebRTC.Data;
using SealWebRTC.Helper;
using SealWebRTC.Hubs;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class ManagerController : Controller
    {
        private readonly EFContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IHubContext<DashHub> _dashHub;
        private readonly ILogger<ManagerController> _logger;

        // private OpenTokService opentokService;

        public ManagerController(
        EFContext context,
        IHubContext<TicketHub> hubContext,
         IHubContext<DashHub> dashHub,
        //OpenTokService _opentokService,
        IHostingEnvironment environment,
        ILogger<ManagerController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _dashHub = dashHub;
            hostingEnvironment = environment;
            _logger = logger;
            //opentokService = _opentokService;
        }

        [Route("MainManager")]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Salas()
        {
            //ViewBag.Meetings = await _context.Meetings
            //        .Include(m => m.TypeAttention)
            //        .Include(m => m.UserClientNavigation)
            //        .Where(m => m.Type == 1 && m.Status == (int)StatusMeeting.TicketEspera &&
            //            m.Created.Year == DateTime.Now.Year &&
            //            m.Created.Month == DateTime.Now.Month &&
            //            m.Created.Day == DateTime.Now.Day)
            //        .ToListAsync();
            return View();
        }

        class Sala
        {
            public String Attention { get; set; }
            public String Client { get; set; }
            public String Number { get; set; }
            public String Created { get; set; }
            public String UniqueId { get; set; }
        }

        [HttpPost]
        public async Task<JsonResult> GetSalas()
        {
            //var options = new JsonSerializerOptions()
            //{
            //    MaxDepth = 0,
            //    IgnoreNullValues = true,
            //    IgnoreReadOnlyProperties = true
            //};

            var meetings = await _context.Meetings
                    .Include(m => m.TypeAttention)
                    .Include(m => m.UserClient)
                    .Where(m => m.Type == 1 && m.Status == (int)StatusMeeting.Espera &&
                        m.Created.Year == DateTime.Now.Year &&
                        m.Created.Month == DateTime.Now.Month &&
                        m.Created.Day == DateTime.Now.Day)
                    .ToListAsync();

            var list = new List<Sala>();

            foreach (var item in meetings)
            {
                var obj = new Sala();
                obj.Attention = item.TypeAttention.Name;
                obj.Client = item.UserClient.FirstName + " " + item.UserClient.LastName;
                obj.Number = item.Number.ToString();
                obj.Created = item.Created.ToString("yyyy-MM-dd HH:mm");
                obj.UniqueId = item.UniqueId.ToString();
                list.Add(obj);
            }

            return Json(list);
            //return Content(list, "application/json"); //JsonSerializer.Serialize(meetings, options);
        }

        [HttpPost]
        public async Task<JsonResult> GetSalasProgramadas()
        {

            var user = HttpContext.Session.GetInt32("Id");
            var meetings = await _context.Meetings
                    .Include(m => m.TypeAttention)
                    .Include(m => m.UserClient)
                    .Where(m => m.Type == 2 && m.Status == (int)StatusMeeting.Espera && (int)m.UserManagerId == user &&
                        m.Created.Year == DateTime.Now.Year &&
                        m.Created.Month == DateTime.Now.Month &&
                        m.Created.Day == DateTime.Now.Day)
                    .ToListAsync();





            var list = new List<Sala>();

            foreach (var item in meetings)
            {
                var obj = new Sala();
                obj.Attention = item.TypeAttention.Name;
                obj.Client = item.UserClient.FirstName + " " + item.UserClient.LastName;
                obj.Number = item.Number.ToString();
                obj.Created = item.Created.ToString("yyyy-MM-dd HH:mm");
                obj.UniqueId = item.UniqueId.ToString();
                list.Add(obj);
            }

            return Json(list);
            //return Content(list, "application/json"); //JsonSerializer.Serialize(meetings, options);
        }

        public async Task<ActionResult> Meeting(String UniqueId)
        {
            await _hubContext.Clients.All.SendAsync("AddTicket", "");




            var meeting = await _context.Meetings.Include(m => m.UserClient).FirstOrDefaultAsync(m => m.UniqueId == UniqueId);


            if (meeting.Status != (int)StatusMeeting.Atencion)
            {

                meeting.UserManagerId = (int)HttpContext.Session.GetInt32("Id");
                meeting.Status = (int)StatusMeeting.Atencion;
                meeting.DurationBegin = DateTime.Now;
                _context.Update(meeting);
                await _context.SaveChangesAsync();
                await _dashHub.Clients.All.SendAsync("UpdateDash");

                ViewBag.Messages = await _context.Messages.Where(m => m.MeetingId == meeting.Id).ToListAsync();
                ViewBag.UserManagerId = meeting.UserManagerId;

                User gestor = await _context.Users.FindAsync(meeting.UserManagerId);

                ViewBag.ChannelName = gestor.ChannelName;
                ViewBag.AccessKeyId = gestor.AccessKeyId;
                ViewBag.SecretAccessKey = gestor.SecretAccessKey;

                OpenTokService opentokService = new OpenTokService();
                opentokService.Generar(gestor.AccessKeyId, gestor.SecretAccessKey);

                ViewBag.ApiKey = opentokService.OpenTok.ApiKey.ToString();
                ViewBag.SessionId = opentokService.Session.Id;
                String token = opentokService.Session.GenerateToken();
                ViewBag.Token = token;


                //var str = JsonConvert.SerializeObject(opentokService);
                //var openTok_str = JsonConvert.SerializeObject(opentokService.OpenTok);
                //var session_str = JsonConvert.SerializeObject(opentokService.Session);
                HttpContext.Session.SetInt32("ApiKey", opentokService.Session.ApiKey);
                HttpContext.Session.SetString("SessionId", opentokService.Session.Id);
                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("ApiSecret", opentokService.Session.ApiSecret);
            }
            else
            {
                ViewBag.ApiKey = HttpContext.Session.GetInt32("ApiKey");
                ViewBag.SessionId = HttpContext.Session.GetString("SessionId");
                ViewBag.Token = HttpContext.Session.GetString("Token");

                ViewBag.Messages = await _context.Messages.Where(m => m.MeetingId == meeting.Id).ToListAsync();
                ViewBag.UserManagerId = meeting.UserManagerId;

                User gestor = await _context.Users.FindAsync(meeting.UserManagerId);

                ViewBag.ChannelName = gestor.ChannelName;
                ViewBag.AccessKeyId = gestor.AccessKeyId;
                ViewBag.SecretAccessKey = gestor.SecretAccessKey;
            }
            return View(meeting);
        }

        [HttpPost]
        public async Task<String> InitMeeting(String UniqueId, String PeerManager, String sessionId, String apiKey, String token)
        {
            await _hubContext.Clients.All.SendAsync("InitMeeting", UniqueId, PeerManager, sessionId, apiKey, token);
            return "Ok";
        }


        public async Task<ActionResult> Cita()
        {
            var UserManagerId = (int)HttpContext.Session.GetInt32("Id");
            return View(await _context.Meetings.Include(m => m.UserClient)
            .Include(m => m.TypeAttention).Where(m => m.UserManagerId == UserManagerId).ToListAsync());
        }

        public async Task<ActionResult> CitaDetalle(int? id)
        {
            var meeting = await _context.Meetings
            .Include(m => m.UserManager)
            .Include(m => m.UserClient)
            .Include(m => m.TypeAttention)
            .Include(m => m.Archives)
            .Include(m => m.Messages)
                .FirstOrDefaultAsync(m => m.Id == id);

            return View(meeting);
        }

        //[HttpPost]
        //public async Task<JsonResult> GetFiles(Guid UserUniqueId, Guid MeetingUniqueId)
        //{
        //    var archives = _context.Archives
        //    .Include(m => m.User)
        //    .Include(m => m.Meeting)
        //    .Where(m => m.User.UniqueId == UserUniqueId && m.Meeting.UniqueId == MeetingUniqueId).ToList();
        //    var list = new List<GetArchives>();
        //    foreach (var item in archives)
        //    {
        //        var archive = new GetArchives();
        //        archive.Name = item.Name;
        //        archive.Path = item.Path;
        //        archive.Created = item.Created.ToString("yyyy-MM-dd HH:mm");
        //        list.Add(archive);
        //    }
        //    return Json(list);
        //}




        [HttpPost]
        public IActionResult Start(String OpenTokSessionId)
        {
            try
            {
                OpenTokService opentokService = new OpenTokService();
                opentokService.Generar(HttpContext.Session.GetInt32("ApiKey").ToString(), HttpContext.Session.GetString("ApiSecret"));
                opentokService.Session.Id = HttpContext.Session.GetString("SessionId");

                OpenTokSDK.Archive archive = opentokService.OpenTok.StartArchive(
                        opentokService.Session.Id,
                        name: "video",
                        hasAudio: true,
                        hasVideo: true,
                        outputMode: OutputMode.COMPOSED//(this.Request.Form.outputMode == "composed" ? OutputMode.COMPOSED : OutputMode.INDIVIDUAL)

                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok(ex.Message);// + "  --  " + str);//OpenTokSessionId + "  --  " + opentokService.Session.Id);
            }

            return Ok("ok");
        }

        [HttpPost]
        public async Task<IActionResult> Stop(String MeetingUniqueId, String archiveID)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == MeetingUniqueId);
            meeting.Status = (int)StatusMeeting.Cerrado;
            meeting.PeerClient = archiveID;
            meeting.DurationEnd = DateTime.Now;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            ///await _dashHub.Clients.All.SendAsync("UpdateDash");
            //await _hubContext.Clients.All.SendAsync("FinishMeeting");


            OpenTokService opentokService = new OpenTokService();
            opentokService.Generar(HttpContext.Session.GetInt32("ApiKey").ToString(), HttpContext.Session.GetString("ApiSecret"));
            opentokService.Session.Id = HttpContext.Session.GetString("SessionId");
            OpenTokSDK.Archive archive = opentokService.OpenTok.StopArchive(archiveID);

            return Ok("ok");
        }

        /*[HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> SaveVideosLocal(String UserUniqueId, String MeetingUniqueId, IFormFile videoLocalBlob)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == MeetingUniqueId);
            meeting.Status = (int)StatusMeeting.Cerrado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            await _dashHub.Clients.All.SendAsync("UpdateDash");
            await _hubContext.Clients.All.SendAsync("FinishMeeting");


            var uploads = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploads, MeetingUniqueId.ToString() + "_local.webm");
            using (var stream = System.IO.File.Create(filePath))
            {
                await videoLocalBlob.CopyToAsync(stream);
            }


            return Ok("ok");
        }*/

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        public async Task<IActionResult> SaveVideosPartner(String UserUniqueId, String MeetingUniqueId, IFormFile videoPartnerBlob)
        {
            var uploads = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploads, MeetingUniqueId.ToString() + "_partner.webm");
            using (var stream = System.IO.File.Create(filePath))
            {
                await videoPartnerBlob.CopyToAsync(stream);
            }




            return Ok("ok");
        }

        public async Task<ActionResult> CancelarCita(String UniqueId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.UniqueId == UniqueId);
            meeting.Status = (int)StatusMeeting.Anulado;
            _context.Update(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction("Salas");
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
            return RedirectToAction("Index", "Manager");
        }

        public async Task<IActionResult> CitaProgramadasAsync()
        {

            DateTime fechaHoy = DateTime.Today;

            var user = HttpContext.Session.GetInt32("Id");
            ViewBag.Meetings = await _context.Meetings
            .Include(m => m.TypeAttention)
            .Where(m => (int)m.UserManagerId == user && 
            m.MeetingDateBegin >= fechaHoy)// && m.Type == 2 && m.MeetingDateBegin > DateTime.Now)
            .ToListAsync();

            return View();
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
