using ClosedXML.Excel;
using ClosedXML.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SealWebRTC.Helper;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class AdminController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(EFContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [Route("MainAdmin")]
        public ActionResult Index()
        {
            var Users = _context.Users.GroupBy(c => c.Rol)
            .Select(g => new { name = g.Key, count = g.Count() }).ToList();
            Dictionary<int, int> usersDic = new Dictionary<int, int>();
            foreach (var item in Users)
            {
                usersDic.Add(Convert.ToInt32(item.name), item.count);
                if (item.name == 1)
                {
                    ViewBag.Clientes = item.count;
                }
                if (item.name == 2)
                {
                    ViewBag.Gestores = item.count;
                }
            }
            ViewBag.Users = usersDic;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetDash(String fecha)
        {
            DateTime beginDate = DateTime.ParseExact(fecha, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var atenciones = _context.Meetings
                .Where(m => m.MeetingDateBegin.Date == beginDate.Date && m.Status != (int)StatusMeeting.Anulado)
                .GroupBy(c => c.Type)
                .Select(g => new { name = g.Key, count = g.Count() });


            Dictionary<int, int> atencionesDic = new Dictionary<int, int>();

            foreach (var item in atenciones)
            {
                atencionesDic.Add(Convert.ToInt32(item.name), item.count);
            }

            //EF.Functions.Like
            //System.Data.Entity.DbFunctions.TruncateTime
            //-------------------------------------------------------------------
            var atencionType = _context.Meetings
                .Include(m => m.TypeAttention)
                .Where(m => m.MeetingDateBegin.Date == beginDate.Date)

            /* .Where(m =>
                 m.MeetingDateBegin.Year == DateTime.Now.Year &&
                 m.MeetingDateBegin.Month == DateTime.Now.Month &&
                 m.MeetingDateBegin.Day == DateTime.Now.Day)*/

                .GroupBy(c => c.TypeAttention.Name)
                .Select(g => new { name = g.Key, count = g.Count() });
            //var atencionTypeDic = new List<(String,String)>();
            List<dynamic> atencionTypeDic = new List<dynamic>();
            decimal sum = 0;
            foreach (var item in atencionType.OrderByDescending(m => m.count))
            {
                var itemList = new { count = item.count.ToString(), name = item.name };
                sum += item.count;
                atencionTypeDic.Add(itemList);
            }
            //-------------------------------------------------------------------

            var atencionesV = _context.Meetings.Include(m => m.TypeAttention)
                    .Include(m => m.UserClient)
                    .Include(m => m.UserManager)
               .Where(m =>
               m.MeetingDateBegin.Year == beginDate.Year &&
               m.MeetingDateBegin.Month == beginDate.Month &&
               m.MeetingDateBegin.Day == beginDate.Day).ToList();

            List<dynamic> atencionesVList = new List<dynamic>();

            double puntaje = 0; int cantidad = 0;

            foreach (var item in atencionesV)
            {
                //var status = "";
                //switch (item.Status)
                //{
                //    case 1: status = "En Espera"; break;
                //    case 2: status = "En Reunion"; break;
                //    case 3: status = "Terminada"; puntaje = Convert.ToDouble(item.Score); cantidad++; break;
                //    case 4: status = "Cancelada"; break;
                //}

                var count = _context.Meetings
              .Where(m =>
              m.MeetingDateBegin.Year == beginDate.Year &&
              m.MeetingDateBegin.Month == beginDate.Month &&
              m.MeetingDateBegin.Day == beginDate.Day &&
              m.UserManagerId == item.UserManagerId).Count();

                DateTime currentDateTime = DateTime.Now;


                TimeSpan sinceNewYear = currentDateTime.Subtract(item.MeetingDateBegin);
                //            Console.WriteLine($"Time since 1 January: {sinceNewYear.Days} Days, " +
                //$"{sinceNewYear.Hours} Hours, {sinceNewYear.Minutes} Minutes, " +
                //$"{sinceNewYear.Seconds} Seconds");


                var itemList = new
                {
                    gestor = item.UserManager == null ? "No Asignado" : (item.UserManager.FirstName + " " + item.UserManager.LastName),
                    cliente = item.UserClient.FirstName + " " + item.UserClient.LastName,
                    atencion = item.TypeAttention.Name,
                    estado = item.Status,
                    puntaje = item.Score,
                    tipo = item.Type == 1 ? "Ticket" : "Cita Programada",
                    cantidad = count,
                    time2 = item.MeetingDateBegin.ToString("yyyy-MM-dd HH:mm:ss"),
                    time = (sinceNewYear.Hours < 10 ? "0" + sinceNewYear.Hours : sinceNewYear.Hours.ToString()) + ":" +
                    (sinceNewYear.Minutes < 10 ? "0" + sinceNewYear.Minutes : sinceNewYear.Minutes.ToString())

                };

                cantidad++;
                puntaje = puntaje + Convert.ToDouble(item.Score);
                atencionesVList.Add(itemList);
            }

            var puntajeTotal = cantidad == 0 ? "0" : (puntaje / cantidad).ToString("0.00");

            return Json(new
            {
                atenciones = atencionesDic,
                AtencionType = atencionTypeDic,
                sum = sum,
                atencionesV = atencionesVList,
                puntaje = puntajeTotal
            });
        }


        // GET: AdminController/Details/5
        public ActionResult Supervicion()
        {
            return View();
        }

        public ActionResult AsignacionCitas()
        {
            ViewBag.Atenciones = _context.Meetings
            .Include(m => m.UserClient)
            .Include(m => m.UserManager)
            .Include(m => m.TypeAttention)
            .Where(m => m.Type == 2 && m.MeetingDateBegin >= DateTime.Now.AddDays(-1) && m.Status != 4)
            .ToList();

            return View();
        }

        public async Task<ActionResult> AsignarGestor(int mettingId)
        {


            ViewData["UserId"] = _context.Users.Where(m => m.Rol == 2).ToArray().Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = String.Format("{0} - {1}", m.FirstName, m.LastName)
            });

            var meeting = await _context.Meetings
                .Include(m => m.UserManager)
                .Include(m => m.UserClient)
                .Include(m => m.TypeAttention)
                    .FirstOrDefaultAsync(m => m.Id == mettingId);
            return View(meeting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AsignarGestor(int mettingId, int userId)
        {
            var meeting = await _context.Meetings
                    .Include(m => m.UserManager)
                    .Include(m => m.UserClient)
                    .Include(m => m.TypeAttention)
                        .FirstOrDefaultAsync(m => m.Id == mettingId);

            meeting.UserManagerId = userId;
            _context.Update(meeting);
            await _context.SaveChangesAsync();

            return RedirectToAction("AsignacionCitas");
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
            return RedirectToAction("Index", "Admin");
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

        public ActionResult DashBoard()
        {
            var Users = _context.Users.GroupBy(c => c.Rol)
            .Select(g => new { name = g.Key, count = g.Count() }).ToList();
            Dictionary<int, int> usersDic = new Dictionary<int, int>();
            foreach (var item in Users)
            {
                usersDic.Add(Convert.ToInt32(item.name), item.count);
            }
            ViewBag.Users = usersDic;
            //-------------------------------------------------------------------
            var atenciones = _context.Meetings.GroupBy(c => c.Type)
           .Select(g => new { name = g.Key, count = g.Count() });
            Dictionary<int, int> atencionesDic = new Dictionary<int, int>();

            foreach (var item in atenciones)
            {
                atencionesDic.Add(Convert.ToInt32(item.name), item.count);
            }
            ViewBag.Atenciones = atencionesDic;
            //-------------------------------------------------------------------
            var atencionType = _context.Meetings.Include(m => m.TypeAttention).GroupBy(c => c.TypeAttention.Name)
           .Select(g => new { name = g.Key, count = g.Count() });
            Dictionary<String, int> atencionTypeDic = new Dictionary<String, int>();

            foreach (var item in atencionType.OrderByDescending(m => m.count))
            {
                atencionTypeDic.Add(item.name, item.count);
            }
            ViewBag.AtencionTypeDic = atencionTypeDic;


            return View();
        }

        public IActionResult GetGestor(String search)
        {
            dynamic data = new JObject();
            data.results = new JArray() as dynamic;

            var gestores = _context.Users
                .Where(l => l.Rol == 2 && (l.FirstName.Contains(search) || l.LastName.Contains(search)))
                .Select(x => new
                {
                    id = x.Id,
                    text = x.FirstName + " " + x.LastName
                }).ToList();

            foreach (var gestor in gestores)
            {
                dynamic obj = new JObject();

                obj.id = gestor.id;
                obj.text = gestor.text;

                data.results.Add(obj);
            }
            return Content(data.ToString(), "application/json");
        }

        public async Task<ActionResult> ReportByDateAndUser()
        {
            ViewBag.Attentions = await _context.Typeattentions.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Users = await _context.Users.Where(m => m.Rol == 2).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ReportByDateAndUser(String MeetingDateBegin, String MeetingDateEnd, int UserManagerId, int TypeAttentionId)
        {
            try
            {
                DateTime beginDate = DateTime.ParseExact(MeetingDateBegin, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(MeetingDateEnd, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                List<Meeting> list = new List<Meeting>();

                IQueryable<Meeting> query = _context.Meetings
                    .Include(m => m.TypeAttention)
                    .Include(m => m.UserClient)
                    .Include(m => m.UserManager)
                    .Where(m => m.MeetingDateBegin >= beginDate &&
                                m.MeetingDateEnd <= endDate);

                if (UserManagerId > 0 && TypeAttentionId > 0)
                {
                    list = query.Where(m =>
                                    m.UserManagerId == UserManagerId &&
                                    m.TypeAttentionId == TypeAttentionId)
                        .ToList();
                }
                else if (UserManagerId > 0)
                {
                    list = query.Where(m =>
                                    m.UserManagerId == UserManagerId)
                        .ToList();
                }
                else if (TypeAttentionId > 0)
                {
                    list = query.Where(m =>
                                    m.TypeAttentionId == TypeAttentionId)
                        .ToList();
                }
                else
                {
                    list = _context.Meetings
                            .Include(m => m.TypeAttention)
                            .Include(m => m.UserClient)
                            .Include(m => m.UserManager)
                            .Where(m => m.MeetingDateBegin >= beginDate &&
                                        m.MeetingDateEnd <= endDate)
                            .ToList();
                }


                //List<Meeting> list = new List<Meeting>();

                //if (UserManagerId > 0 && TypeAttentionId > 0)
                //{
                //    list = _context.Meetings
                //        .Include(m => m.TypeAttention)
                //        .Include(m => m.UserClient)
                //        .Include(m => m.UserManager)
                //        .Where(m => m.MeetingDateBegin >= beginDate &&
                //                    m.MeetingDateEnd <= endDate &&
                //                    m.UserManagerId == UserManagerId &&
                //                    m.TypeAttentionId == TypeAttentionId)
                //        .ToList();
                //}
                //else if (UserManagerId > 0)
                //{
                //    list = _context.Meetings
                //        .Include(m => m.TypeAttention)
                //        .Include(m => m.UserClient)
                //        .Include(m => m.UserManager)
                //        .Where(m => m.MeetingDateBegin >= beginDate &&
                //                    m.MeetingDateEnd <= endDate &&
                //                    m.UserManagerId == UserManagerId)
                //        .ToList();
                //}
                //else if (TypeAttentionId > 0)
                //{
                //    list = _context.Meetings
                //        .Include(m => m.TypeAttention)
                //        .Include(m => m.UserClient)
                //        .Include(m => m.UserManager)
                //        .Where(m => m.MeetingDateBegin >= beginDate &&
                //                    m.MeetingDateEnd <= endDate &&
                //                    m.TypeAttentionId == TypeAttentionId)
                //        .ToList();
                //}
                //else
                //{
                //    list = _context.Meetings
                //            .Include(m => m.TypeAttention)
                //            .Include(m => m.UserClient)
                //            .Include(m => m.UserManager)
                //            .Where(m => m.MeetingDateBegin >= beginDate &&
                //                        m.MeetingDateEnd <= endDate)
                //            .ToList();
                //}

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Reporte");

                int cont = 2;

                ws.Range("A" + cont, "H" + cont).Style.Fill.SetBackgroundColor(XLColor.FromArgb(79, 129, 189));
                ws.Range("A" + cont, "H" + cont).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick);
                ws.Range("A" + cont, "H" + cont).Style.Border.SetOutsideBorderColor(XLColor.FromArgb(149, 179, 215));
                ws.Range("A" + cont, "H" + cont).Style.Font.SetFontColor(XLColor.White);

                ws.Cells("A" + cont).Value = "Fecha";
                ws.Cells("B" + cont).Value = "Gestor";
                ws.Cells("C" + cont).Value = "Cliente";
                ws.Cells("D" + cont).Value = "Cliente Email";
                ws.Cells("E" + cont).Value = "Tipo de atencion";
                ws.Cells("F" + cont).Value = "Ticket / Programada";
                ws.Cells("G" + cont).Value = "Estado";
                ws.Cells("H" + cont).Value = "Puntaje";

                if (list != null)
                {
                    foreach (var item in list)
                    {
                        cont++;
                        ws.Cell("A" + cont).Value = item.Created.ToString("dd/MM/yyyy");
                        ws.Cell("A" + cont).Style.NumberFormat.Format = "dd/MM/yyyy";
                        ws.Cell("B" + cont).Value = (item.UserManager == null) ? "Sin Gestor Asignado" : item.UserManager.FirstName + " " + item.UserManager.LastName;
                        ws.Cell("C" + cont).Value = item.UserClient.FirstName + " " + item.UserClient.LastName;
                        ws.Cell("D" + cont).Value = item.UserClient.Email;
                        ws.Cell("E" + cont).Value = item.TypeAttention.Name;
                        ws.Cell("F" + cont).Value = item.Type == 1 ? "Ticket" : "Cita Programada";
                        String estado;
                        switch (item.Status)
                        {
                            case 0:
                                estado = "Programada";
                                break;
                            case 1:
                                estado = "Espera";
                                break;
                            case 2:
                                estado = "Atencion";
                                break;
                            case 3:
                                estado = "Cerrado";
                                break;
                            case 4:
                                estado = "Anulado";
                                break;
                            default:
                                estado = "Sin estado";
                                break;
                        }
                        ws.Cell("G" + cont).Value = estado;
                        ws.Cell("H" + cont).Value = item.Score == null ? 0 : item.Score;
                    }
                }
                try
                {
                    var rangod = ws.Range("A2:H" + cont); //Seleccionamos un rango
                    rangod.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick); //Generamos las lineas exteriores
                    rangod.Style.Border.SetInsideBorder(XLBorderStyleValues.Medium); //Generamos las lineas interiores
                    rangod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //Alineamos horizontalmente
                    rangod.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;  //Alineamos verticalmente
                    rangod.Style.Font.FontSize = 12; //Indicamos el tamaño de la fuente

                    ws.Columns("A", "h").AdjustToContents();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                return wb.Deliver("Reporte.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return View();
        }

        public async Task<ActionResult> ReportByDateAndScore()
        {
            ViewBag.Users = await _context.Users.Where(m => m.Rol == 2).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ReportByDateAndScore(String MeetingDateBegin, String MeetingDateEnd, int UserManagerId)
        {
            try
            {
                DateTime beginDate = DateTime.ParseExact(MeetingDateBegin, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(MeetingDateEnd, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                //var list = new List<(string Key, int Count, double? Total, double? Average)>();
                //var list = new List<object>();                          


                IQueryable<Meeting> list = _context.Meetings
                       .Include(m => m.UserManager);

                if (UserManagerId > 0)
                {
                    list = list.Where(m => m.MeetingDateBegin >= beginDate &&
                                    m.MeetingDateEnd <= endDate &&
                                    m.UserManagerId == UserManagerId && m.UserManagerId != null);

                }
                else
                {
                    list = list.Where(m => m.MeetingDateBegin >= beginDate &&
                                       m.MeetingDateEnd <= endDate);
                }



                var rpta = list
                .GroupBy(i => i.UserManager.FirstName + " " + i.UserManager.LastName)
                       .Select(m => new
                       {
                           Key = m.Key,
                           Count = m.Count(),
                           Total = m.Sum(i => i.Score),
                           Average = m.Average(i => i.Score)
                       })
                       .ToList();

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Reporte");

                int cont = 2;

                ws.Range("A" + cont, "C" + cont).Style.Fill.SetBackgroundColor(XLColor.FromArgb(79, 129, 189));
                ws.Range("A" + cont, "C" + cont).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick);
                ws.Range("A" + cont, "C" + cont).Style.Border.SetOutsideBorderColor(XLColor.FromArgb(149, 179, 215));
                ws.Range("A" + cont, "C" + cont).Style.Font.SetFontColor(XLColor.White);

                ws.Cells("A" + cont).Value = "Gestor";
                ws.Cells("B" + cont).Value = "Cantidad";
                //ws.Cells("C" + cont).Value = "Sumatoria";
                ws.Cells("C" + cont).Value = "Promedio";

                if (rpta != null)
                {
                    foreach (var item in rpta)
                    {
                        if(item.Key != " ")
                        {
                            cont++;
                            ws.Cell("A" + cont).Value = item.Key;
                            ws.Cell("B" + cont).Value = item.Count;
                            // ws.Cell("C" + cont).Value = item.Total;
                            ws.Cell("C" + cont).Value = Convert.ToDouble(item.Average).ToString("#.00");
                        }
                        
                    }
                }
                try
                {
                    var rangod = ws.Range("A2:C" + cont); //Seleccionamos un rango
                    rangod.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick); //Generamos las lineas exteriores
                    rangod.Style.Border.SetInsideBorder(XLBorderStyleValues.Medium); //Generamos las lineas interiores
                    rangod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //Alineamos horizontalmente
                    rangod.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;  //Alineamos verticalmente
                    rangod.Style.Font.FontSize = 12; //Indicamos el tamaño de la fuente

                    ws.Columns("A", "C").AdjustToContents();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                return wb.Deliver("Reporte_Puntaje.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return View();
        }

        public async Task<ActionResult> ReportByDateAndAtention()
        {
            ViewBag.Attentions = await _context.Typeattentions.OrderBy(m => m.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ReportByDateAndAtention(String MeetingDateBegin, String MeetingDateEnd, int TypeAttentionId)
        {
            try
            {
                DateTime beginDate = DateTime.ParseExact(MeetingDateBegin, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(MeetingDateEnd, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);

                //var list = new List<(string Key, int Count, double? Total, double? Average)>();
                //var list = new List<object>();                          


                IQueryable<Meeting> list = _context.Meetings
                       .Include(m => m.TypeAttention);


                if (TypeAttentionId > 0)
                {
                    list = list.Where(m => m.MeetingDateBegin >= beginDate &&
                                    m.MeetingDateEnd <= endDate &&
                                    m.TypeAttentionId == TypeAttentionId && m.UserManagerId != null);

                }
                else
                {
                    list = list.Where(m => m.MeetingDateBegin >= beginDate &&
                                       m.MeetingDateEnd <= endDate);
                }



                var rpta = list
                .GroupBy(i => i.TypeAttention.Name)
                       .Select(m => new
                       {
                           Key = m.Key,
                           Count = m.Count(),
                           //Total = m.Sum(i => i.Score),
                           // Average = m.Average(i => i.Score)
                       })
                       .ToList();

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Reporte");

                int cont = 2;

                ws.Range("A" + cont, "B" + cont).Style.Fill.SetBackgroundColor(XLColor.FromArgb(79, 129, 189));
                ws.Range("A" + cont, "B" + cont).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick);
                ws.Range("A" + cont, "B" + cont).Style.Border.SetOutsideBorderColor(XLColor.FromArgb(149, 179, 215));
                ws.Range("A" + cont, "B" + cont).Style.Font.SetFontColor(XLColor.White);

                ws.Cells("A" + cont).Value = "Tipo de Atencion";
                ws.Cells("B" + cont).Value = "Cantidad";
                //ws.Cells("C" + cont).Value = "Sumatoria";
                //ws.Cells("D" + cont).Value = "Promedio";

                if (rpta != null)
                {
                    foreach (var item in rpta)
                    {
                        cont++;
                        ws.Cell("A" + cont).Value = item.Key;
                        ws.Cell("B" + cont).Value = item.Count;
                        //ws.Cell("C" + cont).Value = item.Total;
                        //ws.Cell("D" + cont).Value = item.Average;
                    }
                }
                try
                {
                    var rangod = ws.Range("A2:B" + cont); //Seleccionamos un rango
                    rangod.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick); //Generamos las lineas exteriores
                    rangod.Style.Border.SetInsideBorder(XLBorderStyleValues.Medium); //Generamos las lineas interiores
                    rangod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //Alineamos horizontalmente
                    rangod.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;  //Alineamos verticalmente
                    rangod.Style.Font.FontSize = 12; //Indicamos el tamaño de la fuente

                    ws.Columns("A", "B").AdjustToContents();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                return wb.Deliver("Reporte_Tipo_Atencion.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return View();
        }


    }
}
