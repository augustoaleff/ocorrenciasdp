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
    [Admin]
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
        public IActionResult Filtrar(string nome, string setor, int? page)
        {
            ViewBag.Setores = setores;
            var pageNumber = page ?? 1;

            var query = _db.Int_Dp_Usuarios
                .Join(_db.Int_DP_Setores, u => u.Setor.Id, o => o.Id, (u,o) => new { u, o })
                .Where(u => u.u.Ativo == 1)
                .AsQueryable();

            if(nome != null)
            {
                query = query.Where(a => a.u.Nome.ToLower().Contains(nome.ToLower()));
            }

            if(setor != null && setor != "0")
            {
                query = query.Where(a => a.u.Setor.Id == int.Parse(setor));

            }

            var relat = query.Select(s => new
            {
                s.u.Id,
                s.u.Login,
                s.u.Nome,
                Setor = s.o.Nome,
                s.u.Perfil,
                s.u.Ativo,
                s.u.UltimoLogin

            }).ToList();

            foreach (var user in relat)
            {
                UsuariosViewModel userVM = new UsuariosViewModel
                {
                    Id = user.Id,
                    Login = user.Login,
                    Nome = user.Nome,
                    Perfil = user.Perfil,
                    Ativo = user.Ativo,
                    Setor = user.Setor,
                    UltimoAcesso = user.UltimoLogin
                };
                usuariosVM.Add(userVM);
            }

            ViewBag.PesquisaSetor = setor;
            ViewBag.PesquisaNome = nome;

            var resultadoPaginado = usuariosVM.ToPagedList(pageNumber, 5);


            return View("Index",resultadoPaginado);

        }


        [HttpGet]
        public IActionResult Index(int? page)
        {
            ViewBag.Setores = setores;
            var pageNumber = page ?? 0;


            var relat = _db.Int_Dp_Usuarios
                .Join(_db.Int_DP_Setores, a => a.Setor.Id, b => b.Id, (a, b) => new { a, b })
                .Where(u => u.a.Ativo == 1)
                .OrderBy(o => o.a.Nome)
                .Select(s => new
                {
                    s.a.Id,
                    s.a.Login,
                    s.a.Nome,
                    s.a.Perfil,
                    s.a.Ativo,
                    Setor = s.a.Setor.Nome,
                    s.a.UltimoLogin
                }).ToList();

            foreach (var user in relat)
            {
                UsuariosViewModel userVM = new UsuariosViewModel
                {
                    Id = user.Id,
                    Login = user.Login,
                    Nome = user.Nome,
                    Perfil = user.Perfil,
                    Ativo = user.Ativo,
                    Setor = user.Setor,
                    UltimoAcesso = user.UltimoLogin
                    
                };
                usuariosVM.Add(userVM);
            }

            var resultadoPaginado = usuariosVM.ToPagedList(pageNumber, 5);

            return View(resultadoPaginado);

        }

        [HttpGet]
        public ActionResult Excluir(int id)
        {
            
            var usuario = _db.Int_Dp_Usuarios.Find(id);
            string usuario_temp = usuario.Login;
            usuario.Ativo = 0;
            usuario.Login = string.Concat(usuario.Login,DateTime.Now.Day.ToString(),DateTime.Now.Second.ToString(),DateTime.Now.Minute.ToString());
            //_db.Int_Dp_Usuarios.Remove(usuario);
            _db.SaveChanges();

            TempData["UsuarioExcluido"] = "O usuário '" + usuario_temp + "' foi excluido!";

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Atualizar(int id)
        {
            Usuario usuario = _db.Int_Dp_Usuarios.Find(id);

            usuario.Login = usuario.Login.ToLower();

            ViewBag.User = usuario;
            ViewBag.ConfirmaSenha = usuario.Senha;
            ViewBag.Setores2 = setores2;

            return View("Cadastrar");
        }


        [HttpPost]
        public ActionResult Atualizar([FromForm]Usuario usuario, string confirmasenha)
        {
            var vSetor = _db.Int_DP_Setores.Find(usuario.Setor.Id);
            usuario.Setor = vSetor;

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;

            if (ModelState.IsValid)
            {

                var vUsuario = _db.Int_Dp_Usuarios.Find(usuario.Id);
                if (vUsuario.Login.ToLower() != usuario.Login.ToLower())
                {
                    var vUsuario2 = _db.Int_Dp_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                    if (vUsuario2 == null)
                    {
                        usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                        usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Senha
                        confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Confirmação da Senha
                        usuario.Email = usuario.Email.ToLower();

                        if(usuario.Senha == confirmasenha) { 

                        var vUpdate = _db.Int_Dp_Usuarios.Find(usuario.Id);

                        vUpdate.Login = usuario.Login;
                        vUpdate.Nome = usuario.Nome;
                        vUpdate.Perfil = usuario.Perfil;
                        vUpdate.Senha = usuario.Senha;
                        vUpdate.Setor = usuario.Setor;
                        vUpdate.Ativo = usuario.Ativo;
                        vUpdate.Email = usuario.Email;
                        
                        _db.SaveChanges();
                        TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi atualizado com sucesso!";
                        return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["SenhaNaoConfere"] = "Senhas não conferem!";
                            ViewBag.User = usuario;
                            return View("Cadastrar");
                        }
                    }
                    else
                    {
                        TempData["ExisteUsuario"] = "Já existe um usuário com esse login, favor escolher outro!";
                        ViewBag.User = usuario;
                        return View("Cadastrar");
                    }

                }
                else
                {
                    usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                    usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Senha
                    confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Confirmação da Senha

                    if (usuario.Senha == confirmasenha)
                    {

                        var vUpdate = _db.Int_Dp_Usuarios.Find(usuario.Id);

                        vUpdate.Login = usuario.Login;
                        vUpdate.Nome = usuario.Nome;
                        vUpdate.Perfil = usuario.Perfil;
                        vUpdate.Senha = usuario.Senha;
                        vUpdate.Setor = usuario.Setor;
                        vUpdate.Ativo = usuario.Ativo;
                        vUpdate.Email = usuario.Email;

                        _db.SaveChanges();

                        TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi atualizado com sucesso!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["SenhaNaoConfere"] = "Senhas não conferem!";
                        ViewBag.User = usuario;
                        return View("Cadastrar");
                    }
                }
            }

            return View("Cadastrar");

        }


        [HttpGet]
        public ActionResult Cadastrar()
        {

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;
            return View();

        }

        [HttpPost]
        public ActionResult Cadastrar([FromForm]Usuario usuario, string confirmasenha)
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
                    usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Senha
                    confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").ToLower(); //Passa para minúsculo a Confirmação da Senha
                    usuario.Email = usuario.Email.ToLower();
                    usuario.UltimoLogin = DateTime.Now;

                    if(usuario.Senha == confirmasenha) { 

                    _db.Int_Dp_Usuarios.Add(usuario);
                    _db.SaveChanges();
                    TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi cadastrado com sucesso!";
                    return RedirectToAction("Index");

                    }
                    else
                    {
                        TempData["SenhaNaoConfere"] = "Senhas não conferem!";
                        ViewBag.User = usuario;
                        return View();
                    }
                
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