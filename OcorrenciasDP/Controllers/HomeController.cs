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
        //const string SessionName = "_Nome";
        //const string SessionID = "_Cod";

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
                //Verifica se o login existe no banco
                var vLogin = _db.Int_Dp_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                //Se existir ele entra no if
                if (vLogin != null) {

                    //Verifica se está ativo
                    if (vLogin.Ativo == 1)
                    {
                        //Verifica se a senha está correta
                        if (Equals(vLogin.Senha, usuario.Senha.ToLower()))
                        {
                            //Envia para a página
                            HttpContext.Session.SetString("Login", vLogin.Nome);
                            HttpContext.Session.SetString("Acesso", vLogin.Perfil);
                            HttpContext.Session.SetString("Setor", vLogin.Setor);
                            HttpContext.Session.SetInt32("ID", vLogin.Id);
                            return RedirectToAction("Inicio", "Home"); //Vai para a página de Início

                        }
                        else
                        {
                            TempData["MensagemErro"] = "Senha incorreta";
                            return View(usuario);
                        }
                    }
                    else
                    {
                        TempData["MensagemErro"] = "O Usuário não está Ativo";
                        return View(usuario);
                    }

                } else {
                    TempData["MensagemErro"] = "Usuário não Encontrado";
                    return View(usuario);
                }
                /*

                if (usuario.Login.ToLower() == "aleff" && usuario.Senha.ToLower() == "123456")
                {
                   // HttpContext.Session.SetString(SessionName, "aleff");
                    //HttpContext.Session.SetInt32(SessionID, 123);

                    HttpContext.Session.SetString("Login", "true");
                    return RedirectToAction("Inicio","Home"); //Vai para a página de Início

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
            */
            }

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
            return RedirectToAction("Index","Home");
        }
    }
}
