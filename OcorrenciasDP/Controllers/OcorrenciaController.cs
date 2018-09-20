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
    public class OcorrenciaController : Controller
    {
        private DatabaseContext _db;

        public OcorrenciaController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Ocorrencia = new Ocorrencia();
            return View();
        }

        [HttpPost]
        public ActionResult Atualizar([FromForm]Ocorrencia ocorrencia)
        {

            ocorrencia.Id_usuario = HttpContext.Session.GetInt32("ID");

            if (ModelState.IsValid)
            {

                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = " & Format(Data.Text, 'YYYYMMDD') & ";
                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && o.Id_usuario.Equals(ocorrencia.Id_usuario)).FirstOrDefault();

                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia != null)
                {
                    _db.Int_DP_Ocorrencias.Update(ocorrencia);
                    _db.SaveChanges();
                    TempData["MsgOcorrenciaOK"] = "Ocorrência Atualizada com Sucesso";
                    //ViewBag.Update_OC = "false";
                    return View();
                }

                return View();
            }
            return View();

        }


        [HttpPost]
        public ActionResult Index([FromForm]Ocorrencia ocorrencia)
        {
            ocorrencia.Id_usuario = HttpContext.Session.GetInt32("ID");

            if (ModelState.IsValid)
            {
                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = " & Format(Data.Text, 'YYYYMMDD') & ";
                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && o.Id_usuario.Equals(ocorrencia.Id_usuario)).FirstOrDefault();

                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia == null)
                {
                    _db.Int_DP_Ocorrencias.Add(ocorrencia);
                    _db.SaveChanges();
                    TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";
                    //ViewBag.Update_OC = "false";
                    return View();
                }
                else
                {
                    //### Gerar alerta para o usuário perguntado se ele quer que atualize a pagina, se sim, executa este código, senão, não executa e volta pra View;
                    TempData["MsgOcorrenciaNotOK"] = "Já existe uma ocorrencia cadastrada para esta data!";
                    //ViewBag.Update_OC = "true";
                    return View();
                }
            }



            return View();
        }

    }
}