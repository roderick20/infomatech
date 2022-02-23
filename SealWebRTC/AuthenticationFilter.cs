using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SealWebRTC
{
    public class AuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            String path = context.HttpContext.Request.Path;

            List<String> path_anonymus = new List<string>();
            path_anonymus.Add("/Login");
            path_anonymus.Add("/Logout");
            path_anonymus.Add("/RecoveryPassword");
            path_anonymus.Add("/Register");
            path_anonymus.Add("/ConfirmationEmail");
            path_anonymus.Add("/Home/PaginaNoEncontrada");
            path_anonymus.Add("/get-captcha-image");
            path_anonymus.Add("/Test/Email");

            String controllerName = context.RouteData.Values["controller"].ToString();
            String actionName = context.RouteData.Values["action"].ToString();

            String anonymus = path_anonymus.Where(m => m.Equals(path)).FirstOrDefault();
            if (anonymus == null)
            {
                if (!context.HttpContext.Session.GetInt32("Id").HasValue)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Login"
                    }));
                }

                if (context.HttpContext.Session.GetInt32("Rol") == 1)//clientes
                {

                    if (controllerName != "Client")
                    {
                        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Client",
                            action = "PaginaInvalidad"
                        }));
                    }

                }
                else if (context.HttpContext.Session.GetInt32("Rol") == 2)//manager
                {

                }
                else if (context.HttpContext.Session.GetInt32("Rol") == 3)//admin
                {

                }
            }



        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
