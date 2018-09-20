using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Library.Filters;

namespace OcorrenciasDP.Controllers
{
    [Login]
    public class RelatorioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}