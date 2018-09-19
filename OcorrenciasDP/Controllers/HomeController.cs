using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Models;

namespace OcorrenciasDP.Controllers
{
   
    public class HomeController : Controller
    {
 
        private DatabaseContext _db;
        public HomeController(DatabaseContext db)
        {
            _db = db;
        }

        
        public IActionResult Inicio()
        {
            var usuarios = _db.Int_Dp_Usuarios.ToList();
            return View(usuarios);
        }

        [HttpGet]

        public ActionResult Index()
        {
            ViewBag.Usuario = new Usuario();
            return View();
        }

        [HttpPost]

        public ActionResult Index([FromForm]Usuario usuario)
        {
            
            if (ModelState.IsValid) //Se a autenticação é válida
            {
                // var id = _db.Int_Dp_Usuarios.

                if (usuario.Login.ToLower() == "aleff" && usuario.Senha.ToLower() == "123456")
                {
                    HttpContext.Session.SetString("Login", "true");
                    return RedirectToAction("About","Home"); //Vai para a página de Início

                }
                else
                {
                    //ViewBag.Mensagem = "Os dados informados são inválidos!";
                    TempData["MensagemErro"] = "Os dados informados são inválidos!";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        
        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Home", "Index");
        }
    }
}
