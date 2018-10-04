using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        readonly List<Setor> setores = new List<Setor>();

        public HomeController(DatabaseContext db)
        {
            _db = db;

            setores = _db.Int_DP_Setores.ToList();

        }

        [Login]
        public IActionResult Inicio()
        {
            var usuarios = _db.Int_Dp_Usuarios.ToList();

            int userID = HttpContext.Session.GetInt32("ID") ?? 0;

            var relat = _db.Int_DP_Ocorrencias
                .Where(a => a.Usuario.Id == userID)
                .OrderByDescending(b => b.Data)
                .Take(5)
                .ToList();


            List<OcorrenciaViewModel> ocorVM = new List<OcorrenciaViewModel>();

            foreach (var ocor in relat)
            {
                OcorrenciaViewModel ocorrenciasVM = new OcorrenciaViewModel
                {
                    Id = ocor.Id,
                    Descricao = ocor.Descricao,
                    Data = ocor.Data,
                    Anexo = ocor.Anexo

                };

                ocorVM.Add(ocorrenciasVM);
            }

            var relat2 = _db.Int_DP_Mensagens
                        .OrderByDescending(b => b.Data)
                        .Take(5)
                        .ToList();

            List<Mensagem> msgVM = new List<Mensagem>();

            foreach(var msg in relat2)
            {
                Mensagem mensagem = new Mensagem
                {
                    Conteudo = msg.Conteudo,
                    Data = msg.Data,
                    Id = msg.Id,
                    Remetente = msg.Remetente
                };

            msgVM.Add(mensagem);
            }

            ViewBag.Msgs = msgVM;

            return View(ocorVM);
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
                if (vLogin != null)
                {

                    // var vSetor = _db.Int_DP_Setores.Find(vLogin.Setor.Id);
                    // vLogin.Setor = vSetor;

                    //Verifica se está ativo
                    if (vLogin.Ativo == 1)
                    {
                        //Verifica se a senha está correta
                        if (Equals(vLogin.Senha, usuario.Senha.ToLower()))
                        {
                            //Envia para a página
                            HttpContext.Session.SetString("Login", vLogin.Nome);
                            HttpContext.Session.SetString("Acesso", vLogin.Perfil);
                            HttpContext.Session.SetString("Setor", vLogin.Setor.Nome);
                            HttpContext.Session.SetString("Perfil", vLogin.Perfil);
                            HttpContext.Session.SetInt32("ID", vLogin.Id);

                            vLogin.UltimoLogin = DateTime.Now;
                            _db.SaveChanges();
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

                }
                else
                {
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

        [HttpGet]
        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
            {
                TempData["ErroAnexo"] = "O arquivo não foi encontrado!";
                return View("Index");
            }


            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", filename);

            if (System.IO.File.Exists(path)) //Se o arquivo existir
            {


                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {

                    await stream.CopyToAsync(memory);

                }
                memory.Position = 0;
                return File(memory, GetContentType(path), Path.GetFileName(path));

            }
            else // Se o arquivo não existir
            {
                TempData["ErroAnexo"] = "O Arquivo não foi encontrado!";
                return RedirectToAction("Index");
            }
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
