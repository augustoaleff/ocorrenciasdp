using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
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

        //Visualizar as palavras
        public IActionResult Index(int? page)
        {
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

            /*  SELECT O.DATA,O.DESCRICAO,O.ID,U.NOME,U.SETOR 
             *  FROM INT_DP_OCORRENCIAS AS O 
             *  INNER JOIN INT_DP_USUARIOS AS U 
             *  ON O.ID_USUARIO = U.ID
             *  ORDER BY O.DATA */


            var relat = _db.Int_DP_Ocorrencias
                .Join(_db.Int_Dp_Usuarios, o => o.Usuario.Id, u => u.Id, (o, u) => new { o,u })
                .Select(s => new { s.o.Data,
                    s.o.Descricao, 
                    s.o.Id, 
                    s.u.Nome, 
                    s.u.Setor}).ToList();

            var resultadoPaginado = relat.ToPagedList(pageNumber, 5);

            

            return View(resultadoPaginado);
        }
    }
}