using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SealWebRTC.Helper;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class ClientesController : Controller
    {
        private readonly EFContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(EFContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Users.Where(m => m.Rol == 1).ToListAsync());
        //}

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IndexData(DataTableRequest request)
        {
            dynamic datatable = new JObject();
            datatable.draw = request.Draw;

            IQueryable<User> count = _context.Users.Where(m => m.Rol == 1).OrderBy(m => m.NumberDoc);

            IQueryable<User> products = _context.Users.Where(m => m.Rol == 1).OrderBy(m => m.NumberDoc);

            if (!String.IsNullOrEmpty(request.Search.Value))
            {
                count = count
                    .Where(m => 
                    m.TypeDoc.Contains(request.Search.Value) ||
                    m.NumberDoc.Contains(request.Search.Value) ||
                    m.FirstName.Contains(request.Search.Value) ||
                    m.LastName.Contains(request.Search.Value) ||
                    m.Email.Contains(request.Search.Value) ||
                    m.Phone.Contains(request.Search.Value) ||
                    m.Created.ToString().Contains(request.Search.Value) ||
                    m.LastAccess.ToString().Contains(request.Search.Value)
                );
                products = products
                    .Where(m =>
                    m.TypeDoc.Contains(request.Search.Value) ||
                    m.NumberDoc.Contains(request.Search.Value) ||
                    m.FirstName.Contains(request.Search.Value) ||
                    m.LastName.Contains(request.Search.Value) ||
                    m.Email.Contains(request.Search.Value) ||
                    m.Phone.Contains(request.Search.Value) ||
                    m.Created.ToString().Contains(request.Search.Value) ||
                    m.LastAccess.ToString().Contains(request.Search.Value)
                );
            }

            if (!String.IsNullOrEmpty(request.Columns[1].Search.Value))
            {
                count = count.Where(m => m.NumberDoc.Contains(request.Columns[1].Search.Value));
                products = products.Where(m => m.NumberDoc.Contains(request.Columns[1].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[2].Search.Value))
            {
                count = count.Where(m => m.FirstName.Contains(request.Columns[2].Search.Value));
                products = products.Where(m => m.FirstName.Contains(request.Columns[2].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[3].Search.Value))
            {
                count = count.Where(m => m.LastName.Contains(request.Columns[3].Search.Value));
                products = products.Where(m => m.LastName.Contains(request.Columns[3].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[4].Search.Value))
            {
                count = count.Where(m => m.Email.Contains(request.Columns[4].Search.Value));
                products = products.Where(m => m.Email.Contains(request.Columns[4].Search.Value));
            }
            if (!String.IsNullOrEmpty(request.Columns[5].Search.Value))
            {
                count = count.Where(m => m.Phone.Contains(request.Columns[5].Search.Value));
                products = products.Where(m => m.Phone.Contains(request.Columns[5].Search.Value));
            }

            datatable.recordsTotal = count.Count();
            int contFilter = products.Count();
            datatable.recordsFiltered = contFilter;
            var listFiltered = products.Skip(request.Start).Take(request.Length).ToList();

            datatable.data = new JArray() as dynamic;
            foreach (var item in listFiltered)
            {
                dynamic obj = new JObject();

                obj.TypeDoc = item.TypeDoc;
                obj.NumberDoc = item.NumberDoc;
                obj.FirstName = item.FirstName;
                obj.LastName = item.LastName;
                obj.Email = item.Email;
                obj.CellPhone = item.Phone;
                obj.Created = item.Created.ToString("yyyy-MM-dd");
                obj.LastAccess = item.LastAccess.ToString("yyyy-MM-dd");

                datatable.data.Add(obj);

            }
            return Content(datatable.ToString(), "application/json");
        }

        public IActionResult ClientsDay()
        {
            return View();
        }

        public class RegistroAgrupado
        {
            public DateTime Fecha { get; set; }
            public int Cantidad { get; set; }
        }

        public async Task<IActionResult> ClientsDayData(DataTableRequest request)
        {
            dynamic datatable = new JObject();
            datatable.draw = request.Draw;

            var countTotal = _context.Users
                .Where(m => m.Rol == 1)
                .GroupBy(m => m.Created.Date)
                .Select(m => new RegistroAgrupado { Fecha = m.Key, Cantidad = m.Count() });

            IQueryable<RegistroAgrupado> countFiltrado;

            if (!String.IsNullOrEmpty(request.Search.Value) && request.Search.Value.Length == 10)
            {
                DateTime Fecha = DateTime.ParseExact(request.Search.Value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                countFiltrado = _context.Users
                    .Where(m => m.Rol == 1 && m.Created.Date == Fecha.Date)
                    .GroupBy(m => m.Created.Date)
                    .Select(m => new RegistroAgrupado { Fecha = m.Key, Cantidad = m.Count() });
            }
            else
            {
                countFiltrado = _context.Users
                    .Where(m => m.Rol == 1)
                    .GroupBy(m => m.Created.Date)
                    .Select(m => new RegistroAgrupado { Fecha = m.Key, Cantidad = m.Count() });
            }

            countFiltrado = countFiltrado.OrderByDescending(m => m.Fecha);

            datatable.recordsTotal = countTotal.Count();
            int contFilter = countFiltrado.Count();
            datatable.recordsFiltered = contFilter;
            var listFiltered = countFiltrado.Skip(request.Start).Take(request.Length).ToList();

            datatable.data = new JArray() as dynamic;
            foreach (var item in listFiltered)
            {
                dynamic obj = new JObject();

                obj.Fecha = item.Fecha.ToString("yyyy-MM-dd");
                obj.Cantidad = item.Cantidad;

                datatable.data.Add(obj);
            }

            return Content(datatable.ToString(), "application/json");
        }
    }
}
