using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
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
        
        public MensagensController(DatabaseContext db)
        {
            _db = db;
        }

        public List<Mensagem> ConsultarMensagens() {
            var relat2 = _db.Int_DP_Mensagens
                       .OrderByDescending(b => b.Data)
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

            }
                ViewBag.Msgs = msgVM;

            return msgVM;
        }

        [HttpPost]
        public ActionResult Lembrete(int dias)
        {
            if(dias > 0)
            {

                List<int> vUsuariosSemEnvio = new List<int>();

                try {

                    DateTime datainicio = (DateTime.Today.Date.AddDays(dias * (-1)));

                    vUsuariosSemEnvio = _db.Int_DP_Ocorrencias
                                       .Where(a => a.Data >= datainicio && a.Data <= DateTime.Today.Date)
                                       .GroupBy(g => g.Usuario.Id)
                                       .Select(s => s.Key)
                                       .ToList();

                }catch (InvalidOperationException)
                {
                    TempData["LembreteNotOK"] = "Todos os usuários já enviaram as ocorrências no periodo solicitado!";
                    return RedirectToAction("Index");
                }

                var vUsuarios = _db.Int_Dp_Usuarios
                                .Where(a => a.Ativo == 1)
                                .Select(s => s.Id)
                                .ToList();

                var lista2 = vUsuarios.Except(vUsuariosSemEnvio).ToList();


                if (lista2.Count > 0)
                {
                    List<string> vEmails = new List<string>();

                    foreach (var id in lista2)
                    {
                        var email = _db.Int_Dp_Usuarios
                                     .Where(a => a.Id == id)
                                     .Select(s => s.Email)
                                     .FirstOrDefault();

                        if (!vEmails.Contains(email)) { //Para não entrar duplicado
                        vEmails.Add(email);
                        
                        }
                    }


                    vEmails.RemoveAll(item => item == null); //remove os valores nulos da lista

                    
                    if(vEmails.Count > 0) { 

                        EnviarLembrete.EnviarMsgLembrete(dias, vEmails);

                    }

                    TempData["LembreteOK"] = "Lembrete Enviado!";
                    return RedirectToAction("Index");

                }
                else
                {
                    TempData["LembreteNotOK"] = "Todos os usuários já enviaram email no periodo solicitado!";
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;

            List<Mensagem> msgVM = ConsultarMensagens();

            var resultadoPaginado = msgVM.ToPagedList(pageNumber, 10);
            return View("Index", resultadoPaginado);
        }

        [HttpGet]
        public ActionResult DetalharMsg(long? id, int? page)
        {
            int pageNumber = page ?? 1;

            var vMensagem = _db.Int_DP_Mensagens.Find(id);
            if (vMensagem.Titulo == null)
            {
                vMensagem.Titulo = "Sem Título";
            }

            ViewBag.DetalheMsg = vMensagem;
            ViewBag.MsgConteudo2 = vMensagem.Conteudo.Replace("\r\n", " <br /> ");

            List<Mensagem> msgVM = ConsultarMensagens();

            var resultadoPaginado = msgVM.ToPagedList(pageNumber, 10);
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

            var vRemetente = _db.Int_Dp_Usuarios.Find(idNotNull);
            mensagem.Remetente = vRemetente;
            mensagem.Data = DateTime.Now;
            
            if(ModelState.IsValid == true)
            {

                _db.Int_DP_Mensagens.Add(mensagem);
                _db.SaveChanges();

                TempData["MensagemEnviada"] = "Mesnagem enviada com sucesso!";

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