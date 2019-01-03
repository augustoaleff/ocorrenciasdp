using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Library.Globalization;
using OcorrenciasDP.Library.Mail;
using OcorrenciasDP.Models;
using X.PagedList;

namespace OcorrenciasDP.Controllers
{

    [Login]
    [Admin]
    public class MensagensController : Controller
    {

        private DatabaseContext _db;
        readonly int paginasPagedList = 10;

        public MensagensController(DatabaseContext db)
        {
            _db = db;
        }

        public List<Mensagem> ConsultarMensagens()
        {
            List<Mensagem> relat2 = _db.Int_DP_Mensagens
                       .OrderByDescending(b => b.Data)
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

            }

            ViewBag.Msgs = msgVM;

            return msgVM;
        }

        [HttpPost]
        public ActionResult Lembrete(int dias)
        {
            int id_user = HttpContext.Session.GetInt32("ID") ?? 0;

            if (dias > 0)
            {
                List<DateTime> diasLista = new List<DateTime>();
                List<DateTime> feriados = new List<DateTime>();
                List<int> vUsuariosSemEnvio = new List<int>();
                DateTime hoje = Globalization.HojeBR();

                try
                {
                    DateTime datainicio = (hoje.AddDays(dias * (-1)));

                    vUsuariosSemEnvio = _db.Int_DP_Ocorrencias
                                       .Where(a => a.Data >= datainicio && a.Data < hoje)
                                       .GroupBy(g => g.Usuario.Id)
                                       .Select(s => s.Key)
                                       .ToList();
                }
                catch (InvalidOperationException exp)
                {
                    TempData["LembreteNotOK"] = "Todos os usuários já enviaram as ocorrências no periodo solicitado!";

                    Log log = new Log();
                    log.EnviarLembrete_Erro(id_user, exp);
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }
                catch (Exception exp)
                {
                    TempData["LembreteNotOK"] = "Ocorreu um erro ao tentar enviar o lembrete!";

                    Log log = new Log();
                    log.EnviarLembrete_Erro(id_user, exp);
                    _db.SaveChanges();

                    return RedirectToAction("Index");

                }

                List<int> vUsuarios = _db.Int_DP_Usuarios
                                .Where(a => a.Ativo == 1)
                                .Select(s => s.Id)
                                .ToList();

                List<int> usuariosNaoEnviados = vUsuarios.Except(vUsuariosSemEnvio).ToList();

                if (usuariosNaoEnviados.Count > 0)
                {

                    List<string> vEmails = new List<string>();

                    foreach (var id in usuariosNaoEnviados)
                    {
                        string email = _db.Int_DP_Usuarios
                                     .Where(a => a.Id == id)
                                     .Select(s => s.Email)
                                     .FirstOrDefault();

                        if (!vEmails.Contains(email))
                        {  //Para não entrar duplicado

                            vEmails.Add(email);

                        }
                    }

                    vEmails.RemoveAll(item => item == null); //Remove os valores nulos da lista
                    vEmails.RemoveAll(item => item == string.Empty); //Remove os valores de string vazia
                    vEmails.RemoveAll(item => item == ""); //Remove os valores de string vazia

                    if (vEmails.Count > 0)
                    {
                        Log log = new Log();

                        try
                        {

                            EnviarLembrete.EnviarMsgLembrete(dias, vEmails);
                            TempData["LembreteOK"] = "Lembrete Enviado!";

                            log.EnviarLembrete(id_user, vEmails.Count);
                            _db.Int_DP_Logs.Add(log);

                        }
                        catch (Exception exp)
                        {
                            log.EnviarLembrete_Erro(id_user, exp);
                            _db.Int_DP_Logs.Add(log);

                            TempData["LembreteNotOK"] = "Ocorreu um erro ao tentar enviar o lembrete, por favor, tente novamente!";
                        }
                        finally
                        {
                            _db.SaveChanges();
                        }

                    }
                    else
                    {
                        TempData["LembreteNotOK"] = "Não há e-mails cadastrados para envio";
                    }
                    
                    return RedirectToAction("Index");

                }
                else
                {
                    TempData["LembreteNotOK"] = "Todos os usuários já enviaram as ocorrências no período solicitado!";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["LembreteNotOK"] = "Insira um período de dias válido";
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;

            List<Mensagem> msgVM = ConsultarMensagens();

            var resultadoPaginado = msgVM.ToPagedList(pageNumber, paginasPagedList);
            return View("Index", resultadoPaginado);
        }

        [HttpGet]
        public ActionResult DetalharMsg(long? id, int? page)
        {
            int pageNumber = page ?? 1;

            Mensagem vMensagem = _db.Int_DP_Mensagens.Find(id);
            if (vMensagem.Titulo == null)
            {
                vMensagem.Titulo = "Sem Título";
            }

            ViewBag.DetalheMsg = vMensagem;
            ViewBag.MsgConteudo2 = vMensagem.Conteudo.Replace("\r\n", " <br /> ");

            List<Mensagem> msgVM = ConsultarMensagens();

            IPagedList<Mensagem> resultadoPaginado = msgVM.ToPagedList(pageNumber, paginasPagedList);
            return View("Index", resultadoPaginado);

        }

        [HttpGet]
        public ActionResult Cadastrar()
        {
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public ActionResult Cadastrar([FromForm]Mensagem mensagem)
        {
            int idNotNull = HttpContext.Session.GetInt32("ID") ?? 0;


            Usuario vRemetente = _db.Int_DP_Usuarios.Find(idNotNull);
            mensagem.Remetente = vRemetente;
            mensagem.Data = Globalization.HoraAtualBR();

            if (ModelState.IsValid == true)
            {
                Log log = new Log();

                try
                {
                    _db.Int_DP_Mensagens.Add(mensagem);
                    _db.SaveChanges();

                    log.EnviarMensagem(idNotNull, mensagem.Id);
                    _db.Int_DP_Logs.Add(log);

                    TempData["MensagemEnviada"] = "Mensagem enviada com sucesso!";

                }
                catch (Exception exp)
                {

                    log.EnviarMensagem_Erro(idNotNull, exp);
                    _db.Int_DP_Logs.Add(log);

                    TempData["MensagemNaoEnviada"] = "Ocorreu um erro ao enviar a mensagem, por favor, envie novamente!";
                
                }
                finally
                {
                    _db.SaveChangesAsync();
                }

                return RedirectToAction("Index");

            }
            else
            {
                TempData["MensagemNaoEnviada"] = "Ocorreu um erro ao enviar a mensagem, por favor, envie novamente!";
                return RedirectToAction("Index");

            }


        }
    }


}