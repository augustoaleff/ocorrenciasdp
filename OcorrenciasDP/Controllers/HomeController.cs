using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Library.Globalization;
using OcorrenciasDP.Models;
using OcorrenciasDP.Library.Mail;

namespace OcorrenciasDP.Controllers
{

    public class HomeController : Controller
    {
        private DatabaseContext _db;

        readonly List<Setor> setores = new List<Setor>();

        public HomeController(DatabaseContext db)
        {
            _db = db;
            setores = _db.Int_DP_Setores.ToList();  
        }

        public List<OcorrenciaViewModel> CarregarOcorrencias()
        {
            var usuarios = _db.Int_DP_Usuarios.ToList();
            int userID = HttpContext.Session.GetInt32("ID") ?? 0;

            // Take(5) = SELECT TOP 5

            var relat = _db.Int_DP_Ocorrencias
                .Where(a => a.Usuario.Id == userID)
                .OrderByDescending(b => b.Data)
                .Take(5)
                .ToList();

            List<OcorrenciaViewModel> ocorVM = new List<OcorrenciaViewModel>();

            foreach (Ocorrencia ocor in relat)
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
            List<Mensagem> relat2 = _db.Int_DP_Mensagens
                         .OrderByDescending(b => b.Data)
                         .Take(5)
                         .ToList();

            List<Mensagem> msgVM = new List<Mensagem>();

            foreach (Mensagem msg in relat2)
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

            DateTime dataUltimoAcesso = _db.Int_DP_Usuarios
                                   .Where(a => a.Id == userId)
                                   .Select(s => s.UltimoLogin)
                                   .FirstOrDefault();

            DateTime dataUltimoAcesso2 = Globalization.ConverterData(HttpContext.Session.GetString("UltimoAcesso"));

            List<Mensagem> mensagem = _db.Int_DP_Mensagens
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

            if (HttpContext.Session.GetString("Visualizado") == "false")
            {
                VerificarMensagensNovas();
            }

            return View(ocorVM);
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("Login") == null)
            {
                ViewBag.Usuario = new Usuario();
                return View();
            }
            else
            {
                return RedirectToAction("Inicio");
            }
        }

        [HttpPost]
        public ActionResult Index([FromForm]Usuario usuario)
        {
            if(ModelState.IsValid) //Se a autenticação é válida
            {
                //Verifica se o login existe no banco
                Usuario vLogin = _db.Int_DP_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                //Se existir ele entra no if
                if (vLogin != null)
                {
                    //Verifica se está ativo
                    if (vLogin.Ativo == 1)
                    {
                        //Verifica se a senha está correta
                        if (Equals(vLogin.Senha, usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'","").ToLower()))
                        {
                            try
                            {
                                //Envia para a página
                                HttpContext.Session.SetString("Login", vLogin.Nome);
                                HttpContext.Session.SetString("Setor", vLogin.Setor.Nome);
                                HttpContext.Session.SetString("Perfil", vLogin.Perfil);
                                HttpContext.Session.SetInt32("ID", vLogin.Id);
                                HttpContext.Session.SetString("UltimoAcesso", vLogin.UltimoLogin.ToString("dd/MM/yyyy HH:mm:ss"));
                                HttpContext.Session.SetString("Visualizado", "false");

                                int loja = _db.Int_DP_Usuarios.Where(a => a.Id == vLogin.Id).Select(s => s.Loja.Id).FirstOrDefault();


                                HttpContext.Session.SetInt32("Loja", loja);
                                
                                vLogin.UltimoLogin = Globalization.HoraAtualBR();

                                Log log = new Log();
                                log.LogIn(vLogin.Id);
                                _db.Int_DP_Logs.Add(log);

                                _db.SaveChanges();

                                return RedirectToAction("Inicio", "Home"); //Vai para a página de Início

                            }
                            catch (Exception exp)
                            {
                                Log log = new Log();
                                log.LogIn_Erro(vLogin.Id, exp);
                                _db.Int_DP_Logs.Add(log);

                                _db.SaveChanges();
                                HttpContext.Session.Clear(); //Limpa a sessão para voltar ao início
                                TempData["MensagemErro"] = "Ocorreu um erro ao tentar logar";
                                return RedirectToAction("Index", "Home");
                            }
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
                           "wwwroot", "uploads", filename);

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
                return RedirectToAction("Inicio");
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
                {".csv", "text/csv"},
                {".tiff", "image/tiff"}
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
            Mensagem vMensagem = _db.Int_DP_Mensagens.Find(id);
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
        
        [HttpPost]
        public ActionResult TrocarSenha([FromForm]int? id,string nome,string email,string senha,string confirmasenha)
        {
            int id_notnull = id ?? 0;

            ViewBag.Senha = senha;
            ViewBag.Nome = nome;
            ViewBag.Email = email;
            ViewBag.IdSenha = id;

            if (senha.ToLower() == confirmasenha.ToLower())
            {
                Log log = new Log();

                try
                {
                    Usuario vUsuario = _db.Int_DP_Usuarios.Find(id);
                    vUsuario.Senha = senha.ToLower();

                    _db.SaveChanges();

                    log.EsqueciMinhaSenha_Troca(vUsuario.Id);

                    TempData["TrocaSenhaOK"] = "Senha Alterada com sucesso";
                    return RedirectToAction("Index");
                }
                catch (Exception exp)
                {
                    log.EsqueciMinhaSenha_Troca_Erro(id_notnull, exp);

                    TempData["TrocaSenhaNotOK"] = "Ocorreu um Erro ao tentar redefinir a senha, por favor, tente novamente mais tarde!";
                    return View();
                }
                finally
                {
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();
                }
            }
            else
            {
                TempData["SenhaNaoConfere"] = "Senhas não conferem!";
                return View();
            }


        }

        public ActionResult TrocarSenha()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TrocarSenha(long? key)
        {
            long key2 = key ?? 1;

            DateTime agora = Globalization.HoraAtualBR();
            var vKey = _db.Int_DP_ValidSenhas.Find(key2);
            
            if(vKey != null)
            {

               if(vKey.DataExpiracao >= agora)
               {

                    if(vKey.Utilizado == 0)
                    {
                        Usuario usuario = _db.Int_DP_Usuarios.Find(vKey.Usuario);
                  
                        vKey.Utilizado = 1;
                        _db.SaveChanges();

                        ViewBag.Senha = "";
                        ViewBag.Nome = usuario.Nome;
                        ViewBag.Email = usuario.Email;
                        ViewBag.IdSenha = usuario.Id;

                        ViewBag.TrocarSenha = vKey;

                        return View();
                    }
                    else
                    {
                        TempData["TrocaSenhaNotOK"] = "Link já Utilizado";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["TrocaSenhaNotOK"] = "Link Expirado";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["TrocaSenhaNotOK"] = "Link Inválido";
                return RedirectToAction("Index");

            }
        }

        [HttpPost]
        public ActionResult EnviarLinkSenha([FromForm]string email)
        {
            string codigo;
            DateTime agora = Globalization.HoraAtualBR();
            
            email.ToLower();

            var vEmail = _db.Int_DP_Usuarios.Where(a => a.Email == email && a.Ativo == 1).FirstOrDefault();

            if (vEmail != null)
            {
                codigo = vEmail.Id.ToString() + agora.Minute.ToString() + agora.Month.ToString() + agora.Day.ToString() +
                         agora.Year.ToString() + agora.Second.ToString() + agora.Hour.ToString();

                ValidacaoSenha valid = new ValidacaoSenha
                {
                    Id = long.Parse(codigo),
                    Data = agora,
                    DataExpiracao = agora.AddHours(5),
                    Utilizado = 0,
                    Usuario = vEmail.Id
                };

                _db.Int_DP_ValidSenhas.Add(valid);
                _db.SaveChanges();

                Log log = new Log();

                try
                {

                    Library.Mail.EnviarLinkSenha.EnviarLinkTrocarSenha(email, codigo, vEmail.Nome);

                    log.EsqueciMinhaSenha_Envio(vEmail.Id,codigo);

                    TempData["TrocaSenhaOK"] = "Um link foi enviado ao seu e-mail com instruções para trocar a senha";

                    return RedirectToAction("Index");


                }
                catch(Exception exp)
                {

                    log.EsqueciMinhaSenha_Envio_Erro(vEmail.Id, codigo, exp);

                    TempData["TrocaSenhaNotOK"] = "Ocorreu um erro ao tentar enviar o link, por favor tente novamente";

                    return RedirectToAction("Index");

                }
                finally
                {
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();
                }
                
            }
            else
            {
                TempData["TrocaSenhaNotOK"] = "Esta email não está cadastrado no sistema";

                return RedirectToAction("Index");
            }
        }

        public ActionResult Logout()
        {
            Log log = new Log();
            int id = HttpContext.Session.GetInt32("ID") ?? 0;

            try
            {
                log.LogOut(id);
                _db.Int_DP_Logs.Add(log);
            }
            catch (Exception exp)
            {
                log.LogOut_Erro(id, exp);
                _db.Int_DP_Logs.Add(log);
            }
            finally
            {
                _db.SaveChanges();
                HttpContext.Session.Clear();
            }

            return RedirectToAction("Index", "Home");


        }
    }
}
