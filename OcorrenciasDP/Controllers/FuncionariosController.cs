using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Library.Globalization;
using OcorrenciasDP.Models;
using OcorrenciasDP.ViewModels;
using X.PagedList;

namespace OcorrenciasDP.Controllers
{
    [Admin]
    [Login]
    public class FuncionariosController : Controller
    {
        private DatabaseContext _db;

        public FuncionariosController(DatabaseContext db)
        {
            _db = db;
        }

        public IActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;




            var relat = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Usuarios, a => a.Encarregado.Id, b => b.Id, (a, b) => new { a, b })
                        .Where(u => u.a.Ativo == 1)
                        .OrderBy(o => o.a.Nome)
                        .Select(s=> new
                        {
                            s.a.Nome,
                            s.a.Id,
                            s.a.Experiencia,
                            Encarregado = s.b.Nome
                        })
                        .ToList();

            List<FuncionarioViewModel> relatVM = new List<FuncionarioViewModel>();
            
            foreach(var func in relat)
            {
                FuncionarioViewModel funcVM = new FuncionarioViewModel
                {
                    Id = func.Id,
                    Nome = func.Nome,
                    Encarregado = func.Encarregado,
                    Experiencia = func.Experiencia
                };

                relatVM.Add(funcVM);
            }


            IPagedList<FuncionarioViewModel> resultadoPaginado = relatVM.ToPagedList(pageNumber, 10);

            
            return View();
            
        }

        public ActionResult Cadastrar()
        {
            return View();
        }



        

    }
}