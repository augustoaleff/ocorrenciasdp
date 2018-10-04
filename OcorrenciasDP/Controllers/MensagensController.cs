using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Models;

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

        public IActionResult Index()
        {

            
            return View();
        }

        [HttpGet]
        public ActionResult Detalhar(long? id)
        {
            var vMensagem = _db.Int_DP_Mensagens.Find(id);
            ViewBag.MsgDetalhe = vMensagem;
            return View(vMensagem.Conteudo);
        }

        [HttpGet]
        public ActionResult Cadastrar()
        {

            return View("Index");
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

                return View("Index");

            }
            else
            {
                TempData["MensagemNaoEnviada"] = "Ocorreu um erro ao enviar a mensagem, por favor, envie novamente!";
                return View("Index");

            }


        }
    }


}