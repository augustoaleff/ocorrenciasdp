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

        public List<OcorrenciaViewModel> CarregarOcorrencias()
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

            return ocorVM;
        }

        public void ProcurarMensagens()
        {
            var relat2 = _db.Int_DP_Mensagens
                       .OrderByDescending(b => b.Data)
                       .Take(5)
                       .ToList();

            List<Mensagem> msgVM = new List<Mensagem>();

            foreach (var msg in relat2)
            {
                Mensagem mensagem = new Mensagem
                {

                    Conteudo = msg.Conteudo,
                    Data = msg.Data,
                    Id = msg.Id,
                    Remetente = msg.Remetente
                };


                if (msg.Titulo != null)
                {
                    mensagem.Titulo = msg.Titulo;

                }
                else
                {
                    mensagem.Titulo = "Sem Título";
                }

                msgVM.Add(mensagem);


                ViewBag.Msgs = msgVM;
            }
        }

        public void VerificarMensagensNovas()
        {



                int userId = HttpContext.Session.GetInt32("ID") ?? 0;

                var dataUltimoAcesso = _db.Int_Dp_Usuarios
                                       .Where(a => a.Id == userId)
                                       .Select(s => s.UltimoLogin)
                                       .FirstOrDefault();

            DateTime dataUltimoAcesso2 = DateTime.Parse(HttpContext.Session.GetString("UltimoAcesso"));
                
                var mensagem = _db.Int_DP_Mensagens
                                .Where(a => a.Data >= dataUltimoAcesso2)
                                .OrderByDescending(b => b.Data)
                                .ToList();

                if (mensagem.Count > 0)
                {
                    ViewBag.NovaMensagem = mensagem;
                }

                HttpContext.Session.SetString("Visualizado", "true");

        }

        [Login]
        public IActionResult Inicio()
        {
            List<OcorrenciaViewModel> ocorVM = CarregarOcorrencias();

            ProcurarMensagens();

            //DateTime dataUltimoAcesso = DateTime.Parse(HttpContext.Session.GetString("UltimoAcesso"));

            if(HttpContext.Session.GetString("Visualizado") == "false")
            {
                VerificarMensagensNovas();
            }

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
                        if (Equals(vLogin.Senha, usuario.Senha.Replace(";","").Replace(",","").Replace(".","").ToLower()))
                        {
                            //Envia para a página
                            HttpContext.Session.SetString("Login", vLogin.Nome);
                            HttpContext.Session.SetString("Setor", vLogin.Setor.Nome);
                            HttpContext.Session.SetString("Perfil", vLogin.Perfil);
                            HttpContext.Session.SetInt32("ID", vLogin.Id);
                            HttpContext.Session.SetString("UltimoAcesso", vLogin.UltimoLogin.ToString());
                            HttpContext.Session.SetString("Visualizado", "false");

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
                           "wwwroot","uploads", filename);

            Request.ToString();

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

        [HttpGet]
        public ActionResult DetalharMsg(long? id)
        {
            var vMensagem = _db.Int_DP_Mensagens.Find(id);
            if (vMensagem.Titulo == null)
            {
                vMensagem.Titulo = "Sem Título";
            }

            ViewBag.MsgDetalhe = vMensagem;
            //ViewBag.MsgDetalhe.Conteudo = vMensagem.Conteudo.Replace("\r\n", " <br /> ");
            ViewBag.MsgConteudo = vMensagem.Conteudo.Replace("\r\n", " <br /> ");
            ProcurarMensagens();
            List<OcorrenciaViewModel> ocorVM = CarregarOcorrencias();
            return View("Inicio", ocorVM);
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
