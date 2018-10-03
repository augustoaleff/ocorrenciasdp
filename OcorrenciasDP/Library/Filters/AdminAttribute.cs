using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Library.Filters
{
    public class AdminAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("Perfil") != "admin")
            {

                if (context.Controller != null)
                {
                    Controller controlador = context.Controller as Controller;
                    controlador.TempData["MensagemErroAdmin"] = "Você não tem permissão para acessar esta página";
                }

                context.Result = new RedirectToActionResult("Inicio", "Home", null);

            }

        }
    }
}
