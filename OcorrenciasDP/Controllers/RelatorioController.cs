using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Library.Globalization;
using OcorrenciasDP.Models;
using OcorrenciasDP.ViewModels;
using Rotativa.AspNetCore;
using X.PagedList;

namespace OcorrenciasDP.Controllers
{
    
    [Login]
    [Admin]
    public class RelatorioController : Controller
    {
        private DatabaseContext _db;
        readonly List<Setor> setores = new List<Setor>();
        readonly List<Loja> lojas = new List<Loja>();
        List<OcorrenciaViewModel> relatorioVM = new List<OcorrenciaViewModel>();
        readonly int paginasPagedList = 20;

        public RelatorioController(DatabaseContext db)
        {
            _db = db;

            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Setor = setores;
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });

            lojas = _db.Int_DP_Lojas.OrderBy(a => a.Nome).ToList();
            ViewBag.Loja = lojas;
            lojas.Add(new Loja() { Id = 0, Nome = "*Todas*" });
        }
        
        public ActionResult Excluir(long id)
        {
            ViewBag.Setor = setores;
            ViewBag.Loja = lojas;
            int user_id = HttpContext.Session.GetInt32("ID") ?? 0;
            Ocorrencia ocorrencia = _db.Int_DP_Ocorrencias.Find(id);

            Log log = new Log();

            try
            {
                //_db.Int_DP_Ocorrencias.Remove(ocorrencia);
                ocorrencia.Ativo = 0;
                _db.SaveChanges();
                
                TempData["ErroRelat"] = "Ocorrencia #" + id + " excluida!";

                log.ExcluirOcorrencia(user_id, id);
                _db.Int_DP_Logs.Add(log);

            }
            catch (Exception exp)
            {

                log.ExcluirOcorrencia_Erro(user_id, id, exp);
                _db.Int_DP_Logs.Add(log);
                TempData["ErroRelat"] = "Ocorreu um erro ao tentar excluir o registro!";

            }
            finally
            {
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Detalhar(long id, int? page, DateTime? datainicio, DateTime? datafim, string setor, string loja)
        {
            ViewBag.PaginaRelat = page ?? 1;
            ViewBag.DataInicioRelat = datainicio;
            ViewBag.DataFimRelat = datafim;
            ViewBag.SetorRelat = setor;
            ViewBag.LojarRelat = loja;

            ViewBag.Setor = setores;

            ViewBag.Loja = lojas;
            Ocorrencia ocorrencia = _db.Int_DP_Ocorrencias.Find(id);

            var relat = _db.Int_DP_Ocorrencias
               .Join(_db.Int_DP_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
               .Join(_db.Int_DP_Setores, a => a.u.Setor.Id, b => b.Id, (a, b) => new { a, b })
               .Join(_db.Int_DP_Lojas, r => r.a.u.Loja.Id, s => s.Id, (r, s) => new { r, s })
               .Where(w => w.r.a.o.Id == id)
               .Select(s => new
               {
                   s.r.a.o.Data,
                   s.r.a.o.DataEnvio,
                   s.r.a.o.Descricao,
                   s.r.a.o.Id,
                   s.r.a.o.Anexo,
                   s.r.a.u.Nome,
                   Setor = s.r.b.Nome,
                   Loja = s.s.Nome,
                   s.r.a.o.Acidente,
                   s.r.a.o.Advertencia,
                   s.r.a.o.Cedo,
                   s.r.a.o.Atrasado,
                   s.r.a.o.Outro
               }).FirstOrDefault();

            OcorrenciaViewModel detalhes = new OcorrenciaViewModel
            {
                Id = relat.Id,
                Data = relat.Data,
                DataEnvio = relat.DataEnvio,
                Descricao = relat.Descricao.Replace("\r\n", "<br />"),
                Nome = relat.Nome,
                Setor = relat.Setor,
                Loja = relat.Loja,
                Anexo = relat.Anexo,
                Advertencia = relat.Advertencia,
                Acidente = relat.Acidente,
                Cedo = relat.Cedo,
                Atrasado = relat.Atrasado,
                Outro = relat.Outro
            };
            return View(detalhes);
        }

        [HttpGet]
        public IActionResult Filtrar(DateTime? datainicio, DateTime? datafim, string setor, string loja, int? page, bool? pdf)
        {
            Log log = new Log();
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;
            FiltrarPesquisaRelatViewModel pesquisa = new FiltrarPesquisaRelatViewModel() { DataInicio = datainicio, DataFim = datafim, Setor = setor, Loja = loja };

            ViewBag.Setores = setores;
            ViewBag.Lojas = lojas;
            int pageNumber = page ?? 1;
            string filtros = "";

            var query = _db.Int_DP_Ocorrencias
                       .Join(_db.Int_DP_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
                       .Join(_db.Int_DP_Setores, a => a.u.Setor.Id, b => b.Id, (a, b) => new { a, b })
                       .Join(_db.Int_DP_Lojas, r => r.a.u.Loja.Id, s => s.Id, (r,s) => new { r, s })
                       .OrderByDescending(c => c.r.a.o.Data)
                       .ThenByDescending(a => a.r.a.o.Id)
                       .AsQueryable();

            if (pesquisa.DataInicio != null)
            {
                if (pesquisa.DataFim != null)
                {
                    query = query.Where(a => a.r.a.o.Data >= pesquisa.DataInicio && a.r.a.o.Data <= pesquisa.DataFim);
                }
                else
                {
                    query = query.Where(a => a.r.a.o.Data >= pesquisa.DataInicio);
                }
            }

            if (pesquisa.Setor != null && pesquisa.Setor != "0")
            {
                query = query.Where(a => a.r.a.u.Setor.Id == int.Parse(pesquisa.Setor));
            }

            if (pesquisa.Loja != null && pesquisa.Loja != "0")
            {
                query = query.Where(a => a.r.a.u.Loja.Id == int.Parse(pesquisa.Loja));
            }

            query = query.Where(y => y.r.a.o.Ativo == 1);

            DateTime datainicio_notnull = pesquisa.DataInicio ?? DateTime.MinValue;

            DateTime datafim_notnull = pesquisa.DataFim ?? DateTime.MaxValue;

            var relat = query.Select(s => new
            {
                s.r.a.o.Data,
                s.r.a.o.Descricao,
                s.r.a.o.Id,
                s.r.a.o.Anexo,
                s.r.a.u.Nome,
                Setor = s.r.a.u.Setor.Nome,
                Loja = s.r.a.u.Loja.Nome
            }).ToList();

            foreach (var linha in relat)
            {
                OcorrenciaViewModel ocorVM = new OcorrenciaViewModel
                {
                    Nome = linha.Nome,
                    Setor = linha.Setor,
                    Loja = linha.Loja,
                    Descricao = linha.Descricao,
                    Data = linha.Data,
                    Id = linha.Id,
                    Anexo = linha.Anexo
                };
                relatorioVM.Add(ocorVM);
            }
            
            if (relatorioVM.Count > 0)
            {
                filtros = GerarFiltros(datainicio_notnull, datafim_notnull, loja, setor);
                relatorioVM[0].DadosPesquisa = filtros; //Armazena os dados que veio do filtro no primeiro index do modelo (.pdf)
            }
            
            Setor vPesquisa = _db.Int_DP_Setores.Find(int.Parse(pesquisa.Setor));

            Loja vPesquisa2 = _db.Int_DP_Lojas.Find(int.Parse(pesquisa.Loja));

            if (vPesquisa != null)
            {
                ViewBag.NomeSetor = vPesquisa.Nome;
            }
            else
            {
                ViewBag.NomeSetor = "*Todos*";
            }

            if (vPesquisa2 != null)
            {
                ViewBag.NomeLoja = vPesquisa2.Nome;
            }
            else
            {
                ViewBag.NomeLoja = "*Todas*";
            }

            if (pdf != true)
            {
                ViewBag.Pesquisa = pesquisa;

                try
                {
                    IPagedList<OcorrenciaViewModel> resultadoPaginado = relatorioVM.ToPagedList(pageNumber, paginasPagedList);

                    log.ConsultarRelatorio(id_notnull, filtros);
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();

                    return View("Index", resultadoPaginado);
                }
                catch (Exception exp)
                {
                    log.ConsultarRelatorio_Erro(id_notnull, filtros, exp);
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();

                    TempData["ErroRelat"] = "Ocorreu um erro ao tentar consultar o relatório...";

                    return View("Index");

                }
            }
            else
            {
                try
                {
                    string data = Globalization.DataRelatorioPdfBR();

                    ViewAsPdf relatorioPDF = new ViewAsPdf
                    {
                        WkhtmlPath = "~/OcorrenciasDP/wwwroot/Rotativa",
                        ViewName = "VisualizarComoPDF",
                        IsGrayScale = false,
                        Model = relatorioVM,
                        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                        CustomSwitches = "--page-offset 0 --footer-left " + data + " --footer-right [page]/[toPage] --footer-font-size 8",
                        PageSize = Rotativa.AspNetCore.Options.Size.A4

                    };

                    log.ExportarRelatorio(id_notnull, filtros);
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();

                    return relatorioPDF;

                }
                catch (Exception exp)
                {
                    log.ExportarRelatorio_Erro(id_notnull, exp);
                    _db.Int_DP_Logs.Add(log);
                    _db.SaveChanges();

                    TempData["ErroRelat"] = "Ocorreu um erro ao tentar exportar o relátório, por favor, tente novamente...";
        
                    return RedirectToAction("Index");
                }
            }
        }

        public string GerarFiltros(DateTime datainicio, DateTime datafim, string setor, string loja)
        {
            string filtros = "";
            
            if (datainicio != null && datainicio != DateTime.MinValue)
            {
                if (filtros == "")
                {
                    filtros += "Data Inicial: " + Globalization.DataCurtaBR(datainicio);
                }
                else
                {
                    filtros += ", Data Inicial: " + Globalization.DataCurtaBR(datainicio);
                }
            }

            if (datafim != null && datafim != DateTime.MaxValue)
            {
                if (filtros == "")
                {
                    filtros += "Data Final: " + Globalization.DataCurtaBR(datafim);
                }
                else
                {
                    filtros += ", Data Final: " + Globalization.DataCurtaBR(datafim);
                }
            }

            if (setor != null && setor != "0")
            {
                Setor nome_setor = _db.Int_DP_Setores.Find(int.Parse(setor));

                if (filtros == "")
                {
                    filtros += "Setor: " + nome_setor.Nome;
                }
                else
                {
                    filtros += ", Setor: " + nome_setor.Nome;
                }

            }
            else
            {
                if (filtros == "")
                {
                    filtros += "Setor: Todos";
                }
                else
                {
                    filtros += ", Setor: Todos";
                }
            }

            if (loja != null && loja != "0")
            {
                Loja nome_loja = _db.Int_DP_Lojas.Find(int.Parse(loja));

                if (filtros == "")
                {
                    filtros += "Loja: " + nome_loja.Nome;
                }
                else
                {
                    filtros += ", Loja: " + nome_loja.Nome;
                }

            }
            else
            {
                if (filtros == "")
                {
                    filtros += "Loja: Todas";
                }
                else
                {
                    filtros += ", Loja: Todas";
                }
            }

            return filtros;
        }

        [HttpGet]
        public IActionResult Index(int? page)
        {

            ViewBag.Setores = setores;
            ViewBag.Lojas = lojas;
            
            ViewBag.Pesquisa = new FiltrarPesquisaRelatViewModel();

            int pageNumber = page ?? 1;
            Log log = new Log();
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;
            string filtros = GerarFiltros(DateTime.MinValue, DateTime.MaxValue, "0", "0");

            /*  SELECT O.DATA,O.DESCRICAO,O.ID,U.NOME,R.SETOR, L.LOJA
             *  FROM INT_DP_OCORRENCIAS AS O 
             *  INNER JOIN INT_DP_USUARIOS AS U ON O.ID_USUARIO = U.ID
             *  INNER JOIN INT_DP_SETORES AS R ON U.SETORID = R.ID
             *  INNER JOIN INT_DP_LOJAS AS L ON U.LOJAID = L.ID
             *  ORDER BY O.DATA */

            try
            {
                if (relatorioVM.Count == 0)
                {
                    var relat = _db.Int_DP_Ocorrencias
                       .Join(_db.Int_DP_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
                       .Join(_db.Int_DP_Setores, a => a.u.Setor.Id, b => b.Id, (a, b) => new { a, b })
                       .Join(_db.Int_DP_Lojas, r => r.a.u.Loja.Id, s => s.Id, (r, s) => new { r, s })
                       .Where(a => a.r.a.o.Ativo == 1)
                       .OrderByDescending(c => c.r.a.o.Data)
                       .ThenByDescending(a => a.r.a.o.Id)
                       .Select(s => new
                       {
                           s.r.a.o.Data,
                           s.r.a.o.Descricao,
                           s.r.a.o.Id,
                           s.r.a.o.Anexo,
                           s.r.a.u.Nome,
                           Setor = s.r.a.u.Setor.Nome,
                           Loja = s.r.a.u.Loja.Nome
                       }).ToList();

                    foreach (var linha in relat)
                    {
                        OcorrenciaViewModel ocorVM = new OcorrenciaViewModel
                        {

                            Nome = linha.Nome,
                            Loja = linha.Loja,
                            Setor = linha.Setor,
                            Descricao = linha.Descricao,
                            Data = linha.Data,
                            Id = linha.Id,
                            Anexo = linha.Anexo
                            
                        };
                        relatorioVM.Add(ocorVM);
                    }
                }
                
                IPagedList<OcorrenciaViewModel> resultadoPaginado = relatorioVM.ToPagedList(pageNumber, paginasPagedList);

                log.ConsultarRelatorio(id_notnull, filtros);
                _db.Int_DP_Logs.Add(log);
                _db.SaveChanges();

                return View(resultadoPaginado);
            }
            catch (Exception exp)
            {
                log.ConsultarRelatorio_Erro(id_notnull, filtros, exp);
                _db.Int_DP_Logs.Add(log);
                _db.SaveChanges();

                TempData["ErroRelat"] = "Ocorreu um erro ao tentar consultar o relatório...";

                return View();

            }
        }
            
        [HttpGet]
        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
            {
                TempData["ErroRelat"] = "O arquivo não foi encontrado!";
                return View("Index");
            }

            string path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", "uploads", filename);

            if (System.IO.File.Exists(path)) //Se o arquivo existir
            {
                MemoryStream memory = new MemoryStream();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;
                return File(memory, GetContentType(path), Path.GetFileName(path));
            }
            else // Se o arquivo não existir
            {
                TempData["ErroRelat"] = "O Arquivo não foi encontrado!";
                return RedirectToAction("Index");
            }
        }
            
        private string GetContentType(string path)
        {
            Dictionary<string,string> types = GetMimeTypes();
            string ext = Path.GetExtension(path).ToLowerInvariant();
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
                {".wmv", "audio/wave"},
                {".mp3", "audio/mpeg"},
                {".mp4", "video/mp4"},
                {".tif", "image/tiff"},
                {".tiff", "image/tiff"},
                {".zip", "application/zip"},
                { ".wav", "audio/x-wav"},
            };
        }
    }
}