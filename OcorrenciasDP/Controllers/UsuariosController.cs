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
    public class UsuariosController : Controller
    {
        private DatabaseContext _db;
        readonly List<Setor> setores = new List<Setor>();
        readonly List<Setor> setores2 = new List<Setor>(); //Lista sem o "*Todos*"
        List<UsuariosViewModel> usuariosVM = new List<UsuariosViewModel>();

        public UsuariosController(DatabaseContext db)
        {
            _db = db;
            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });
            ViewBag.Setores = setores;
        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            ViewBag.Setores = setores;
            var pageNumber= page ?? 0;


            var relat = _db.Int_Dp_Usuarios
                .Join(_db.Int_DP_Setores, a => a.Setor.Id, b => b.Id, (a, b) => new { a, b })
                .OrderBy(o => o.a.Nome)
                .Select(s => new
                {
                    s.a.Id,
                    s.a.Login,
                    s.a.Nome,
                    s.a.Perfil,
                    s.a.Ativo,
                    Setor = s.a.Setor.Nome
                }).ToList();
            
            foreach(var user in relat)
            {
                UsuariosViewModel userVM = new UsuariosViewModel
                {
                    Id = user.Id,
                    Login = user.Login,
                    Nome = user.Nome,
                    Perfil = user.Perfil,
                    Ativo = user.Ativo,
                    Setor = user.Setor
                };
                usuariosVM.Add(userVM);
            }

            var resultadoPaginado = usuariosVM.ToPagedList(pageNumber, 10);

           
            return View(resultadoPaginado);

        }

        [HttpGet]
        public ActionResult Cadastrar()
        {
            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;
            return View();
        }


        [HttpPost]
        public ActionResult Cadastrar([FromForm]Usuario usuario) {


            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;

            if (ModelState.IsValid)
            {

                var vUsuario = _db.Int_Dp_Usuarios.Where(a => a.Login.Equals(usuario.Login));


                if (vUsuario == null)
                {

                }
                else
                {
                    TempData["ExisteUsuario"] = "Já existe um usuário com esse login, favor escolher outro!";
                    ViewBag.User = usuario;
                    return View();
                }

            }

            return View();
            
        }

    }
}