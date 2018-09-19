using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OcorrenciasDP.Controllers
{
    public class RelatorioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}