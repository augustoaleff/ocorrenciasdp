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
            return View(new Ocorrencia());
        }

        /*public ActionResult Atualizar(int id)
        {
            Ocorrencia ocorrencia = _db.Int_DP_Ocorrencias.Find(id);


        }*/

        [HttpPost]
        public ActionResult Atualizar([FromForm]Ocorrencia ocorrencia)
        {
            //ocorrencia.Id_usuario = HttpContext.Session.GetInt32("ID");

            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario = _db.Int_Dp_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;

            if (ModelState.IsValid)
            {
                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = " & Format(Data.Text, 'YYYYMMDD') & ";
                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && (o.Usuario.Id == ocorrencia.Usuario.Id)).FirstOrDefault();

                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia != null)
                {
                    _db.Int_DP_Ocorrencias.Update(ocorrencia);
                    _db.SaveChanges();
                    TempData["MsgOcorrenciaOK"] = "Ocorrência Atualizada com Sucesso";
                    //ViewBag.Update_OC = "false"; 
                    return View("Index");
                }

                return View(ocorrencia);
            }

            return View();
        }


        [HttpPost]
        public ActionResult Index([FromForm]Ocorrencia ocorrencia)
        {
            //ocorrencia.Id_usuario = HttpContext.Session.GetInt32("ID");
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario =_db.Int_Dp_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;

            if (ModelState.IsValid)
            {
                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = " & Format(Data.Text, 'YYYYMMDD') & ";
                //var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && o.Usuario.Id.Equals(ocorrencia.Usuario.Id)).FirstOrDefault();
                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && (o.Usuario.Id == ocorrencia.Usuario.Id)).FirstOrDefault();

                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia == null)
                {
                    ViewBag.Ocorrencia = new Ocorrencia();
                    _db.Int_DP_Ocorrencias.Add(ocorrencia);
                    _db.SaveChanges();
                    TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";
                    //ViewBag.Update_OC = "false";
                    return View("Index",ocorrencia);
                }
                else
                {
                    ViewBag.Ocorrencia= ocorrencia;
                    ViewBag.Ocorrencia.Id = ocorrencia.Id;

                    //### Gerar alerta para o usuário perguntado se ele quer que atualize a pagina, se sim, executa este código, senão, não executa e volta pra View;
                    TempData["MsgOcorrenciaNotOK"] = "Já existe uma ocorrencia cadastrada para esta data!";
                    //ViewBag.Update_OC = "true";
                    //Retorna o valor como Objeto Ocorrencia para a View
                    return View("Index",ocorrencia);
                }
            }

            return View();
        }

    }
}