using System;
using System.Collections.Generic;
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
    public class RelatorioController : Controller
    {
        private DatabaseContext _db;

        public RelatorioController(DatabaseContext db)
        {
            _db = db;
        }

        public ActionResult Excluir(Int64 id)
        {
            var ocorrencia = _db.Int_DP_Ocorrencias.Find(id);
            _db.Int_DP_Ocorrencias.Remove(ocorrencia);
            _db.SaveChanges();

            TempData["OcorrenciaExcluir"] = "Ocorrencia #" + id + " excluida!";
            return RedirectToAction("Index");
        }

        public ActionResult Detalhar(Int64 id)
        {
            var ocorrencia = _db.Int_DP_Ocorrencias.Find(id);

            var relat = _db.Int_DP_Ocorrencias
               .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
               .Where(a => a.o.Id == id)
               .Select(s => new
               {
                   s.o.Data,
                   s.o.Descricao,
                   s.o.Id,
                   s.o.Anexo,
                   s.u.Nome,
                   s.u.Setor
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

        [HttpPost]
        public ActionResult Filtrar([FromForm]FiltrarPesquisaRelatViewModel pesquisa, int? page)
        {

            var pageNumber = page ?? 1;

            var query = _db.Int_DP_Ocorrencias
                       .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
                       .OrderByDescending(a => a.o.Data)
                       .AsQueryable();

            if (pesquisa.DataInicio != null)
            {
                if (pesquisa.DataFim != null)
                {
                    query = query.Where(a => a.o.Data >= pesquisa.DataInicio && a.o.Data <= pesquisa.DataFim);

                }
                else
                {
                    query = query.Where(a => a.o.Data >= pesquisa.DataInicio);
                }
            }

            if (pesquisa.Setor != null && pesquisa.Setor != "*Todos*")
            {
                query = query.Where(a => a.u.Setor.Equals(pesquisa.Setor));
            }

            var relat = query.Select(s => new
            {
                s.o.Data,
                s.o.Descricao,
                s.o.Id,
                s.o.Anexo,
                s.u.Nome,
                s.u.Setor
            }).ToList();

            List<OcorrenciaViewModel> relatorioVM = new List<OcorrenciaViewModel>();

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

            var resultadoPaginado = relatorioVM.ToPagedList(pageNumber, 10);
            ViewBag.Pesquisa = pesquisa;
            return View("Index", resultadoPaginado);


        }

        [HttpGet]
        //Visualizar as palavras
        public IActionResult Index(int? page)
        {
            ViewBag.Pesquisa = new FiltrarPesquisaRelatViewModel();
            var pageNumber = page ?? 1;

            /* var relat = _db.Int_DP_Ocorrencias.OrderByDescending(o => o.Data).ToList();
             var relat = (from o in _db.Int_DP_Ocorrencias
                         from u in _db.Int_Dp_Usuarios
                         where o.Id_usuario == u.Id
                         select o.Id
            */
            /* var relat = _db.Int_DP_Ocorrencias
                  .Join(_db.Int_Dp_Usuarios,
                        o => o.Id_usuario,
                        u => u.Id,
                        (o,u) => new 
                        )*/

            /*var listaClientes = (from Cli in db.Clientes
                                        join Ped in db.Pedidos on Cli.ClienteId equals Ped.ClienteId
                                        select new { Cli.Nome, Cli.Email, Cli.Endereco,
                                                          Ped.DataPedido, Ped.PrecoPedido }).ToList();
             */

            /*  SELECT O.DATA,O.DESCRICAO,O.ID,U.NOME,U.SETOR 
             *  FROM INT_DP_OCORRENCIAS AS O 
             *  INNER JOIN INT_DP_USUARIOS AS U 
             *  ON O.ID_USUARIO = U.ID
             *  ORDER BY O.DATA */


            List<OcorrenciaViewModel> relatorioVM = new List<OcorrenciaViewModel>();

            var relat = _db.Int_DP_Ocorrencias
               .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o, u })
               .OrderByDescending(a => a.o.Data)
               .Select(s => new
               {
                   s.o.Data,
                   s.o.Descricao,
                   s.o.Id,
                   s.o.Anexo,
                   s.u.Nome,
                   s.u.Setor
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

            /* var relat = _db.Int_DP_Ocorrencias
                .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o,u }) 
                .ToList();*/


            //var relat = _db.Int_DP_Ocorrencias.OrderByDescending(o => o.Data).ToList();


            var resultadoPaginado = relatorioVM.ToPagedList(pageNumber, 10);

            return View(resultadoPaginado);
        }
    }
}