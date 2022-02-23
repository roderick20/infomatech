using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SealWebRTC.Helper;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SealWebRTC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EFContext _context;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, EFContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/")]
        [Route("Login")]
        public IActionResult Login() => View();

        [Route("Login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(String email, String password, String captchaCode)
        {

            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(captchaCode))
            {
                ModelState.AddModelError(string.Empty, "Ingrese el email, la contraseña y el codigo captcha");
                return View();
            }

            try
            {
                password = PasswordHash.GetMd5Hash(password);

                if (captchaCode.ToUpper() == HttpContext.Session.GetString("CaptchaCode"))
                {
                    var usuario = _context.Users.Where(m => m.Email == email &&
                    m.Password == password).FirstOrDefault();

                    if (usuario != null)
                    {
                        if (usuario.ConfirmationEmail)
                        {
                            if (usuario.Enabled)
                            {
                                HttpContext.Session.SetInt32("Id", usuario.Id);
                                HttpContext.Session.SetInt32("Rol", usuario.Rol);
                                HttpContext.Session.SetString("Email", usuario.Email);
                                HttpContext.Session.SetString("Name", usuario.FirstName + " " + usuario.LastName);
                                HttpContext.Session.SetString("UniqueId", usuario.UniqueId.ToString());

                                switch (usuario.Rol)
                                {
                                    case 1: return Redirect("/MainClient");
                                    case 2: return Redirect("/MainManager");
                                    case 3: return Redirect("/MainAdmin");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Usuario deshabilitado");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Error correo no confirmado, por favor revise su correo electrónico");
                            return View();
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Email o contraseña invalidos");
                    return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error en captcha");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña invalidos");
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.LogError(ex.Message);
            }
            return View();
        }

        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.SetInt32("Id", 0);
            HttpContext.Session.SetString("Name", "");
            HttpContext.Session.SetString("UniqueId", "");
            HttpContext.Session.Clear();
            return Redirect("/Login");
        }

        [Route("get-captcha-image")]
        public IActionResult GetCaptchaImage()
        {
            int width = 100;
            int height = 36;
            var captchaCode = Captcha.GenerateCaptchaCode();
            var result = Captcha.GenerateCaptchaImage(width, height, captchaCode);
            HttpContext.Session.SetString("CaptchaCode", result.CaptchaCode);
            Stream s = new MemoryStream(result.CaptchaByteData);
            return new FileStreamResult(s, "image/png");
        }

        [Route("Register")]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Register")]
        public async Task<IActionResult> Register([Bind("TypeDoc,NumberDoc,FirstName,LastName,Email,Password,Phone,Suministro")] User user, String CaptchaCode)
        {
            try
            {
                if (String.IsNullOrEmpty(CaptchaCode))
                {
                    ModelState.AddModelError(string.Empty, "Ingrese el codigo captcha");
                    return View(user);
                }

                if (CaptchaCode.ToUpper() == HttpContext.Session.GetString("CaptchaCode"))
                {

                    /*String ErrorMessage = "";
                    if (!ValidatePassword(user.Password, out ErrorMessage))
                    {
                        ModelState.AddModelError(string.Empty, ErrorMessage);
                        return View(user);
                    }*/

                    if (user.Password.Length < 8)
                    {
                        ModelState.AddModelError(string.Empty, "Contraseña mínimo 8 caracteres");
                        return View(user);
                    }

                    var usuariorepetido = _context.Users.Where(m => m.NumberDoc == user.NumberDoc ||
                    m.Email == user.Email).FirstOrDefault();

                    if (usuariorepetido != null)
                    {
                        if (usuariorepetido.NumberDoc == user.NumberDoc)
                        {
                            ModelState.AddModelError(string.Empty, "Número de documento repetido");
                        }

                        if (usuariorepetido.Email == user.Email)
                        {
                            ModelState.AddModelError(string.Empty, "Correo primario repetido");
                        }

                        return View(user);
                    }

                    user.Password = PasswordHash.GetMd5Hash(user.Password);
                    user.UniqueId = Guid.NewGuid().ToString();
                    user.Created = DateTime.Now;
                    user.Modified = DateTime.Now;
                    user.LastAccess = DateTime.Now;
                    user.Status = true;
                    user.ConfirmationEmail = false;
                    user.Recovery = false;
                    user.Rol = 1; // 1 = Cliente, 2 = Gestor, 3 = Admin
                    user.Enabled = true;
                    user.CellPhone = "0000";


                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    ViewData["Save"] = true;

                    //MODULO CORREO
                    var path = _env.WebRootPath + "/plantilla_email/bienvenida_email.html";
                    String fileContents = System.IO.File.ReadAllText(path);
                    fileContents = fileContents.Replace("$$CODEDATA1$$", user.FirstName.Trim() + " " + user.LastName.Trim());
                    fileContents = fileContents.Replace("$$CODELINK$$", "https://videollamada.seal.com.pe/ConfirmationEmail?uniqueid=" + user.UniqueId);
                    //fileContents = fileContents.Replace("$$CODELINK$$", "https://localhost:44383/ConfirmationEmail?uniqueid=" + user.UniqueId);

                    SendEmailOutlook email = new SendEmailOutlook(user.Email, "Seal confirmacion de correo", fileContents, _env.WebRootPath);

                    if (!email.Send_Register())
                    {
                        _logger.LogError(email.Error);
                    }

                    return View(user);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error en captcha");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(user);
        }

        public string CreatePassword(int length = 8)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+=\\[{\\]};:<>|./?,-";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
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

        [HttpGet]
        [Route("ConfirmationEmail")]
        public IActionResult ConfirmationEmail(String uniqueid)
        {
            try
            {
                var usuario = _context.Users.Where(m => m.UniqueId == uniqueid).FirstOrDefault();
                if (usuario != null)
                {
                    usuario.ConfirmationEmail = true;
                    _context.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Codigo invalido");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Codigo invalido");
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.LogError(ex.Message);
            }
            return RedirectToAction("Login", "Home");
        }

        [Route("RecoveryPassword")]
        public IActionResult RecoveryPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("RecoveryPassword")]
        public IActionResult RecoveryPassword(String email, String captchaCode)
        {
            try
            {
                if (String.IsNullOrEmpty(captchaCode))
                {
                    ModelState.AddModelError(string.Empty, "Ingrese el codigo captcha");
                    return View();
                }

                if (captchaCode.ToUpper() == HttpContext.Session.GetString("CaptchaCode"))
                {

                    var usuario = _context.Users.Where(m => m.Email == email).FirstOrDefault();

                    if (usuario != null)
                    {
                        if (!usuario.ConfirmationEmail)
                        {
                            ModelState.AddModelError(string.Empty, "Error correo no confirmado, por favor revise su correo electrónico");
                            return View();
                        }

                        var Usrcontrasena = CreatePassword();// PasswordHash.GetMd5Hash(captchaCodeGenerado);
                        usuario.Password = PasswordHash.GetMd5Hash(Usrcontrasena);
                        usuario.Modified = DateTime.Now;
                        usuario.LastAccess = DateTime.Now;
                        _context.Update(usuario);
                        _context.SaveChanges();



                        //MODULO CORREO
                        var path = _env.WebRootPath + "/plantilla_email/cambiopassword_email.html";
                        String fileContents = System.IO.File.ReadAllText(path);
                        fileContents = fileContents.Replace("$$CODEDATA1$$", usuario.FirstName.Trim() + " " + usuario.LastName.Trim());
                        fileContents = fileContents.Replace("$$CODEDATA2$$", Usrcontrasena);
                        fileContents = fileContents.Replace("$$CODELINK$$", "https://videollamada.seal.com.pe/Login");

                        SendEmailOutlook email_obj = new SendEmailOutlook(usuario.Email, "Recuperar contraseña", fileContents, _env.WebRootPath);
                        email_obj.Send_Change_Password();

                        if (!email_obj.Error.Equals(""))
                        {
                            ModelState.AddModelError(string.Empty, email_obj.Error);
                            return View();
                        }

                        //

                        ViewData["Grabado"] = true;
                        return View();
                    }

                    ModelState.AddModelError(string.Empty, "Error correo no existe");
                    return View();

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error en captcha");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.LogError(ex.Message);
            }
            return View();
        }

        [Route("ChangePassword")]
        public IActionResult ChangePassword() => View();

        [Route("ChangePassword")]
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

        [Route("error/{errorCode}")]
        public IActionResult ErrorPage(int errorCode)
        {
            return View();
        }

        public IActionResult PaginaInvalidad()
        {
            
            return View();
        }

        public IActionResult PaginaNoEncontrada(int? statusCode)
        {
            if (statusCode.HasValue)
            {
                if (statusCode == 404)
                {
                    ViewBag.Mensaje = "Página no encontrada";
                }

                if (statusCode == 500)
                {
                    ViewBag.Mensaje = "Comuníquese con el administrador";
                }
            }

            return View();
        }
    }
}
