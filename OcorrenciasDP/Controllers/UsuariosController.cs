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
        public ActionResult Excluir(int id)
        {
            var usuario =_db.Int_Dp_Usuarios.Find(id);
            _db.Int_Dp_Usuarios.Remove(usuario);
            _db.SaveChanges();

            TempData["UsuarioExcluido"] = "O usuário '" + usuario.Login + "' foi excluido!";

            return RedirectToAction("Index");
        }
        

        [HttpGet]
        public ActionResult Atualizar(int id)
        {
            Usuario usuario = _db.Int_Dp_Usuarios.Find(id);

            usuario.Login = usuario.Login.ToLower();

            ViewBag.User = usuario;
            ViewBag.Setores2 = setores2;

            return View("Cadastrar");
        }


        [HttpPost]
        public ActionResult Atualizar([FromForm]Usuario usuario)
        {
            var vSetor = _db.Int_DP_Setores.Find(usuario.Setor.Id);
            usuario.Setor = vSetor;

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;

            if (ModelState.IsValid)
            {
                var vUsuario = _db.Int_Dp_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                if (vUsuario == null)
                {
                    usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                    usuario.Senha = usuario.Senha.ToLower(); //Passa para minúsculo a Senha

                    _db.Int_Dp_Usuarios.Add(usuario);
                    _db.Int_DP_Ocorrencias.Update
                    _db.SaveChanges();
                    TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi cadastrado com sucesso!";
                    return RedirectToAction("Index");
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


        [HttpGet]
        public ActionResult Cadastrar()
        {

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;
            return View();

        }



        [HttpPost]
        public ActionResult Cadastrar([FromForm]Usuario usuario) {

            var vSetor = _db.Int_DP_Setores.Find(usuario.Setor.Id);
            usuario.Setor = vSetor;

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;

            if (ModelState.IsValid)
            {
                var vUsuario = _db.Int_Dp_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                if (vUsuario == null)
                {
                    usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                    usuario.Senha = usuario.Senha.ToLower(); //Passa para minúsculo a Senha
                    
                    _db.Int_Dp_Usuarios.Add(usuario);
                    _db.SaveChanges();
                    TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi cadastrado com sucesso!";
                    return RedirectToAction("Index");
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