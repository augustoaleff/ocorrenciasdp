using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OcorrenciasDP.Library.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace OcorrenciasDP.Library.Filters
{
    public class HorarioAttribute : ActionFilterAttribute
    {

        DateTime now = Globalization.Globalization.HoraAtualBR();

        public override void OnActionExecuted(ActionExecutedContext context)
        {

            if(now.Hour >= 7 && now.Hour <= 19) //Das 19h às 7h
            {
                if (context.Controller != null)
                {

                    Controller controlador = context.Controller as Controller;
                    controlador.TempData["MensagemErro"] = "Login não permitido aos finais de semana";

                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
            
            if(now.DayOfWeek == DayOfWeek.Saturday) //Sábado
            {
                if (context.Controller != null)
                {
                    Controller controlador = context.Controller as Controller;
                    controlador.TempData["MensagemErro"] = "Login não permitido devido ao horário";
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);

            }

            if(now.DayOfWeek == DayOfWeek.Sunday) //Domingo
            {   
                if(context.Controller != null)
                {
                    Controller controlador = context.Controller as Controller;
                    controlador.TempData["MensagemErro"] = "Login não permitido aos finais de semana";
                }

                context.Result = new RedirectToActionResult("Index","Home",null);
            }
        }
    }
}
