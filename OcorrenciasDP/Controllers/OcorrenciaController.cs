using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        public string idOcorrencia;

        public OcorrenciaController(DatabaseContext db)
        {
            _db = db;
        }


        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Ocorrencia = new Ocorrencia();
            ViewBag.Ocorrencia.Data = DateTime.Now;

            return View(new Ocorrencia());
        }


        public ActionResult Atualizar([FromForm]Ocorrencia ocorrencia)
        {

            //POST - Request.Form
            //GET - Request.QueryString

            //Ocorrencia ocorrencia = new Ocorrencia();

            ocorrencia.Data = DateTime.Parse(Request.Form["data"]);
            ocorrencia.Descricao = Request.Form["descricao"];
            ocorrencia.Anexo = Request.Form["anexo"];

            //return View("Index", ocorrencia); //retorna para o Index

            //ocorrencia.Id_usuario = HttpContext.Session.GetInt32("ID");

            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario = _db.Int_Dp_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;

            if (ModelState.IsValid)
            {
                ViewBag.Ocorrencia = new Ocorrencia();
               
                ViewBag.Ocorrencia.Data = DateTime.Now;
                _db.Int_DP_Ocorrencias.Add(ocorrencia);
                _db.SaveChanges();
                TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";
                return View("Index", ocorrencia);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index([FromForm]Ocorrencia ocorrencia, IFormFile anexo)
        {

            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario = _db.Int_Dp_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;

            if (ModelState.IsValid)
            {
                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = " & Format(Data.Text, 'YYYYMMDD') & ";/
                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && (o.Usuario.Id == ocorrencia.Usuario.Id)).FirstOrDefault();

                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia == null || ViewBag.Update == true)
                {
                    ViewBag.Ocorrencia = new Ocorrencia();
                    ViewBag.Ocorrencia.Data = DateTime.Now;
                    ViewBag.Ocorrencia.Anexo = anexo.FileName;

                    if(anexo != null) { 
                    ocorrencia.Anexo = anexo.FileName;
                    }
                    _db.Int_DP_Ocorrencias.Add(ocorrencia);
                    _db.SaveChanges();

                    idOcorrencia = ocorrencia.Id.ToString() + "_";
                    UploadFile(anexo);
                    TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";

                    return View("Index", ocorrencia);
                }
                else
                {
                    ViewBag.Ocorrencia = ocorrencia;

                    if(anexo != null) { 
                    ViewBag.Ocorrencia.Anexo = anexo.FileName;
                    }

                    //### Gerar alerta para o usuário perguntado se ele quer que atualize a pagina, se sim, executa este código, senão, não executa e volta pra View;
                    TempData["MsgOcorrenciaNotOK"] = "Já existe uma ocorrencia cadastrada para esta data!";
                    //Retorna o valor como Objeto Ocorrencia para a View
                    return View("Index", ocorrencia);
                }

            }
            
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConsultarBanco([FromForm]Ocorrencia ocorrencia)
        {
            if (ModelState.IsValid)
            {
                int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

                Usuario usuario = _db.Int_Dp_Usuarios.Find(id_notnull);

                ocorrencia.Usuario = usuario;


                var vOcorrencia = _db.Int_DP_Ocorrencias.Where(o => o.Data.Equals(ocorrencia.Data) && (o.Usuario.Id == ocorrencia.Usuario.Id)).FirstOrDefault();

                if (vOcorrencia == null || ViewBag.Update == true)
                {

                    ViewBag.Ocorrencia = new Ocorrencia();
                    ViewBag.Ocorrencia.Data = DateTime.Now;

                    return RedirectToAction("Incluir", ocorrencia);
                }
                else
                {

                    ViewBag.Ocorrencia = ocorrencia;
                    TempData["MsgOcorrenciaNotOK"] = "Já existe uma ocorrencia cadastrada para esta data!";
                    return View("Index", ocorrencia);
                }
            }

            return View("Index", ocorrencia);

        }

        //Updload
        public async void UploadFile(IFormFile file)
        {
            if (file != null || file.Length != 0)
            {

                var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot",
                            string.Concat(idOcorrencia,file.FileName));

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

        }





    }
}