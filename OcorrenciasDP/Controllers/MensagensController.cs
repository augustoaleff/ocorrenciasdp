using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
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