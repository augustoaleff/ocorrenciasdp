using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Models;
using OcorrenciasDP.ViewModels;
using X.PagedList;

namespace OcorrenciasDP.Controllers
{

    [Login]
    [Admin]
    public class RelatorioController : Controller
    {
        private DatabaseContext _db;
        readonly List<Setor> setores = new List<Setor>();
        List<OcorrenciaViewModel> relatorioVM = new List<OcorrenciaViewModel>();

        public RelatorioController(DatabaseContext db)
        {
            _db = db;

            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Setor = setores;
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });
        }

        public ActionResult Excluir(Int64 id)
        {
            ViewBag.Setor = setores;
            var ocorrencia = _db.Int_DP_Ocorrencias.Find(id);
            _db.Int_DP_Ocorrencias.Remove(ocorrencia);
            _db.SaveChanges();

            TempData["OcorrenciaExcluir"] = "Ocorrencia #" + id + " excluida!";
            return RedirectToAction("Index");
        }

        public ActionResult Detalhar(Int64 id)
        {
            ViewBag.Setor = setores;
            var ocorrencia = _db.Int_DP_Ocorrencias.Find(id);

            var relat = _db.Int_DP_Ocorrencias
               .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
               .Join(_db.Int_DP_Setores, a => a.u.Setor.Id, b => b.Id, (a, b) => new { a, b })
               .Where(w => w.a.o.Id == id)
               .Select(s => new
               {
                   s.a.o.Data,
                   s.a.o.Descricao,
                   s.a.o.Id,
                   s.a.o.Anexo,
                   s.a.u.Nome,
                   Setor = s.b.Nome,
               }).FirstOrDefault();

            OcorrenciaViewModel detalhes = new OcorrenciaViewModel
            {
                Id = relat.Id,
                Data = relat.Data,
                Descricao = relat.Descricao,
                Nome = relat.Nome,
                Setor = relat.Setor,
                Anexo = relat.Anexo
            };

            return View(detalhes);
        }

        [HttpGet]
        public ActionResult Filtrar(DateTime? datainicio, DateTime? datafim, string setor, int? page)
        {
            FiltrarPesquisaRelatViewModel pesquisa = new FiltrarPesquisaRelatViewModel() { DataInicio = datainicio, DataFim = datafim, Setor = setor };

            ViewBag.Setores = setores;
            var pageNumber = page ?? 1;

            var query = _db.Int_DP_Ocorrencias
                       .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
                       .Join(_db.Int_DP_Setores, a => a.u.Setor.Id, b => b.Id, (a, b) => new { a, b })
                       .OrderByDescending(c => c.a.o.Data)
                       .AsQueryable();

            if (pesquisa.DataInicio != null)
            {
                if (pesquisa.DataFim != null)
                {
                    query = query.Where(a => a.a.o.Data >= pesquisa.DataInicio && a.a.o.Data <= pesquisa.DataFim);

                }
                else
                {
                    query = query.Where(a => a.a.o.Data >= pesquisa.DataInicio);
                }
            }

            if (pesquisa.Setor != null && pesquisa.Setor != "0")
            {
                query = query.Where(a => a.a.u.Setor.Id == int.Parse(pesquisa.Setor));
            }

            var relat = query.Select(s => new
            {
                s.a.o.Data,
                s.a.o.Descricao,
                s.a.o.Id,
                s.a.o.Anexo,
                s.a.u.Nome,
                Setor = s.a.u.Setor.Nome
            }).ToList();

            foreach (var linha in relat)
            {
                OcorrenciaViewModel ocorVM = new OcorrenciaViewModel
                {
                    Nome = linha.Nome,
                    Setor = linha.Setor,
                    Descricao = linha.Descricao,
                    Data = linha.Data,
                    Id = linha.Id,
                    Anexo = linha.Anexo
                };
                relatorioVM.Add(ocorVM);
            }

            var vPesquisa = _db.Int_DP_Setores.Find(int.Parse(pesquisa.Setor));

            if (vPesquisa != null)
            {
                ViewBag.NomeSetor = vPesquisa.Nome;
            }
            else
            {
                ViewBag.NomeSetor = "*Todos*";
            }

            
            ViewBag.Pesquisa = pesquisa;
            var resultadoPaginado = relatorioVM.ToPagedList(pageNumber, 10);
            return View("Index", resultadoPaginado);

        }



        [HttpGet]
        public IActionResult Index(int? page)
        {

            ViewBag.Setores = setores;
            ViewBag.Pesquisa = new FiltrarPesquisaRelatViewModel();
            var pageNumber = page ?? 1;


            /*  SELECT O.DATA,O.DESCRICAO,O.ID,U.NOME,R.SETOR
             *  FROM INT_DP_OCORRENCIAS AS O 
             *  INNER JOIN INT_DP_USUARIOS AS U ON O.ID_USUARIO = U.ID
             *  INNER JOIN INT_DEP_SETORES AS R ON U.SETORID = R.ID
             *  ORDER BY O.DATA */

            if (relatorioVM.Count == 0)
            {

                var relat = _db.Int_DP_Ocorrencias
                   .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
                   .Join(_db.Int_DP_Setores, r => r.u.Setor.Id, s => s.Id, (r, s) => new { r, s })
                   .OrderByDescending(a => a.r.o.Data)
                   .Select(s => new
                   {
                       s.r.o.Data,
                       s.r.o.Descricao,
                       s.r.o.Id,
                       s.r.o.Anexo,
                       s.r.u.Nome,
                       Setor = s.r.u.Setor.Nome
                   }).ToList();

                foreach (var linha in relat)
                {
                    OcorrenciaViewModel ocorVM = new OcorrenciaViewModel
                    {
                        Nome = linha.Nome,
                        Setor = linha.Setor,
                        Descricao = linha.Descricao,
                        Data = linha.Data,
                        Id = linha.Id,
                        Anexo = linha.Anexo
                    };
                    relatorioVM.Add(ocorVM);
                }
            }

            var resultadoPaginado = relatorioVM.ToPagedList(pageNumber, 10);

            return View(resultadoPaginado);
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
                           "wwwroot", filename);

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
                return RedirectToAction("Index");
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
                {".csv", "text/csv"}
            };
        }
    }
}