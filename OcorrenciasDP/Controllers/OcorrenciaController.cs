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
using OcorrenciasDP.Library.Globalization;
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
            ViewBag.Ocorrencia.Data = Globalization.HoraAtualBR();
            ViewBag.Anexo = "";


            OcorrenciasFaltantes();


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

            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario = _db.Int_DP_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;
            ocorrencia.DataEnvio = Globalization.HoraAtualBR();


            if (ModelState.IsValid)
            {
                ViewBag.Ocorrencia = new Ocorrencia();
                ViewBag.Ocorrencia.Data = Globalization.HoraAtualBR();

                Log log = new Log();

                try
                {
                    _db.Int_DP_Ocorrencias.Add(ocorrencia);
                    _db.SaveChanges();

                    OcorrenciasFaltantes();

                    List<DateTime> diasFaltantes = new List<DateTime>();

                    if (ViewBag.Calendario != null)
                    {
                        diasFaltantes = ViewBag.Calendario;

                    }

                    if (diasFaltantes.Count < 3)
                    {
                        TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";
                    }
                    else
                    {
                        TempData["MsgOcorrenciaAlerta"] = "Ocorrencia Cadastrada com sucesso, porém você ainda tem " + diasFaltantes.Count + " ocorrências para enviar!";
                    }

                    log.IncluirOcorrencia(id_notnull, ocorrencia.Id);
                    _db.Int_DP_Logs.Add(log);

                }
                catch (Exception e)
                {
                    //MsgOcorrenciaNotOK já está em uso!
                    TempData["MsgOcorrenciaNotOK2"] = "Ocorreu um erro ao enviar, por favor, tente novamente...";

                    log.IncluirOcorrencia_Erro(id_notnull, e);
                    _db.Int_DP_Logs.Add(log);
                }
                finally
                {
                    _db.SaveChanges();
                }

                return View("Index", ocorrencia);
            }

            return View();

        }

        public void OcorrenciasFaltantes()
        {

            if (HttpContext.Session.GetString("Perfil") != "admin")
            {
                List<DateTime> dias = new List<DateTime>(); //30 últimos dias
                List<DateTime> enviados = new List<DateTime>(); //Ultimas ocorrências enviadas
                List<DateTime> calend = new List<DateTime>(); //Dias - Falta
                List<DateTime> calend_final = new List<DateTime>(); //Calend - Finais de Semana
                
                try
                {
                    int id_user = HttpContext.Session.GetInt32("ID") ?? 0;

                    DateTime dataCadastro = _db.Int_DP_Usuarios
                                            .Where(a => a.Id == id_user)
                                            .Select(s => s.DataCadastro)
                                            .FirstOrDefault();

                    List<DateTime> feriados = _db.Int_DP_Feriados
                                .OrderByDescending(a => a.Data)
                                .Select(s => s.Data)
                                .ToList();

                    int usuario = HttpContext.Session.GetInt32("ID") ?? 0;

                    TimeSpan diff = Globalization.HoraAtualBR().Subtract(dataCadastro); //Diferença de dias entre data do cadastro e hoje
                    DateTime dataInicial;

                    if (diff.Days >= 30)
                    {
                        //dataInicial = DateTime.Today.AddDays(-30);
                        dataInicial = Globalization.HojeBR().AddDays(-30);

                        //Adiciona 30 dias anteriores a hoje na lista
                        for (int i = 29; i >= 0; i--)
                        {
                            dias.Add(Globalization.HojeBR().AddDays(i * -1));
                        }
                    }
                    else
                    {
                        dataInicial = dataCadastro.Date;

                        for (int i = diff.Days; i >= 0; i--)
                        {
                            //dias.Add(DateTime.Today.AddDays(i * -1));
                            dias.Add(Globalization.HojeBR().AddDays(i * -1));
                        }
                    }

                    enviados = _db.Int_DP_Ocorrencias
                               .Where(a => a.Data >= dataInicial && a.Usuario.Id == usuario)
                               .OrderByDescending(a => a.Data)
                               .Select(a => a.Data)
                               .ToList();

                    //Pega os útimos 30 dias

                    calend = dias.Except(enviados).ToList(); //Retira os dias que foram enviados
                    calend = calend.Except(feriados).ToList(); //Retira os feriados

                    //Retira o sábado e o domingo da lista
                    foreach (DateTime dia in calend)
                    {
                        if (!dia.DayOfWeek.Equals(DayOfWeek.Saturday) && !dia.DayOfWeek.Equals(DayOfWeek.Sunday))
                        {
                            calend_final.Add(dia);
                        }
                    }

                    /*
                    foreach(DateTime dia in calend)
                    {
                        if(!dia.DayOfWeek.Equals(DayOfWeek.Saturday) && !dia.DayOfWeek.Equals(DayOfWeek.Sunday))
                        {
                            calend.Remove(dia);
                        }
                    }
                    */

                    //calend_final.Reverse(); //Reverte a ordem das datas para decrescente

                    ViewBag.Calendario = calend_final;

                }
                catch (Exception)
                {
                    ViewBag.Calendario = null;
                }
            }
        }

        [HttpPost]
        public ActionResult Index([FromForm]Ocorrencia ocorrencia, IFormFile anexo, string update)
        {
            if (ocorrencia.Descricao == null)
            {
                ocorrencia.Descricao = "Não houve ocorrências";
            }

            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            Usuario usuario = _db.Int_DP_Usuarios.Find(id_notnull);

            ocorrencia.Usuario = usuario;

            ocorrencia.DataEnvio = Globalization.HoraAtualBR();

            OcorrenciasFaltantes();

            List<DateTime> diasFaltantes = new List<DateTime>();

            if (ViewBag.Calendario != null)
            {
                diasFaltantes = ViewBag.Calendario;
               
            }

            

            if (ModelState.IsValid)
            {
                //SELECT * FROM INT_DP_OCORRENCIAS WHERE DATA = '" & Format(Data.Text, "YYYYMMDD") & "'";/

                Ocorrencia vOcorrencia = _db.Int_DP_Ocorrencias
                                   .Where(o => o.Data.Equals(ocorrencia.Data) && (o.Usuario.Id == ocorrencia.Usuario.Id))
                                   .FirstOrDefault();
                
                //Se for igual a null, não há nenhuma ocorrencia lançada com a data informada
                if (vOcorrencia == null || update == "true")
                {
                    ViewBag.Ocorrencia = new Ocorrencia();
                    ViewBag.Ocorrencia.Data = Globalization.HoraAtualBR();
                    Log log = new Log();

                    if (anexo != null)
                    {
                        ViewBag.Ocorrencia.Anexo = anexo.FileName;
                        ocorrencia.Anexo = anexo.FileName;
                        ViewBag.Anexo = anexo;
                    } 
                    try
                    {
                        _db.Int_DP_Ocorrencias.Add(ocorrencia);
                        _db.SaveChanges();

                        log.IncluirOcorrencia(ocorrencia.Usuario.Id, ocorrencia.Id);
                        _db.Int_DP_Logs.Add(log);

                    }
                    catch (Exception exp)
                    {
                        log.IncluirOcorrencia_Erro(id_notnull, exp);
                        _db.Int_DP_Logs.Add(log);

                        TempData["MsgOcorrenciaNotOK2"] = "Ocorreu um erro ao enviar, por favor, tente novamente...";

                        OcorrenciasFaltantes();

                        return View("Index", ocorrencia);
                    }
                    finally
                    {
                        _db.SaveChanges();
                    }

                    idOcorrencia = ocorrencia.Id.ToString() + "_";

                    if (anexo != null)
                    {
                        UploadFile(anexo);
                    }

                    if (diasFaltantes.Count < 3)
                    {
                        TempData["MsgOcorrenciaOK"] = "Ocorrência Cadastrada com Sucesso";
                    }
                    else
                    {
                        TempData["MsgOcorrenciaAlerta"] = "Ocorrencia Cadastrada com sucesso, porém você ainda tem " + diasFaltantes.Count + " ocorrências para enviar!";
                    }
                    
                    OcorrenciasFaltantes(); //Atualiza as datas

                    return View("Index", ocorrencia);

                }
                else
                {
                    ViewBag.Ocorrencia = ocorrencia;

                    if (anexo != null)
                    {
                        ViewBag.Ocorrencia.Anexo = anexo.FileName;
                    }

                    //### Gerar alerta para o usuário perguntado se ele quer que atualize a pagina, se sim, executa este código, senão, não executa e volta pra View;
                    TempData["MsgOcorrenciaNotOK"] = "Já existe uma ocorrencia cadastrada para esta data!";
                    //Retorna o valor como Objeto Ocorrencia para a View

                    OcorrenciasFaltantes();

                    return View("Index", ocorrencia);
                }
            }

            ViewBag.Ocorrencia = ocorrencia;

            if (anexo != null)
            {
                ViewBag.Ocorrencia.Anexo = anexo.FileName;
            }
            
            OcorrenciasFaltantes();


            return View();
        }


        //Upload
        public async void UploadFile(IFormFile file)
        {
            if (file != null || file.Length != 0)
            {
                string path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot/uploads",
                            string.Concat(idOcorrencia, file.FileName));

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
        }
        
    }
}