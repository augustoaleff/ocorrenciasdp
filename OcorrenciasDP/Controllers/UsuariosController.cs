using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcorrenciasDP.Database;
using OcorrenciasDP.Library.Filters;
using OcorrenciasDP.Library.Globalization;
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
        readonly List<Loja> lojas = new List<Loja>();
        List<UsuariosViewModel> usuariosVM = new List<UsuariosViewModel>();

        public UsuariosController(DatabaseContext db)
        {
            _db = db;
            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();
            lojas.Add(new Loja() { Id = 0, Nome = "*Todas*" });
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });
            ViewBag.Setores = setores;
        }

        [HttpGet]
        public IActionResult Filtrar(string nome, string setor, string loja, int? page)
        {
            ViewBag.Setores = setores;
            ViewBag.Lojas = lojas;
            int pageNumber = page ?? 1;

            var query = _db.Int_DP_Usuarios
                .Join(_db.Int_DP_Setores, u => u.Setor.Id, o => o.Id, (u, o) => new { u, o })
                .Join(_db.Int_DP_Lojas, r => r.u.Loja.Id, l => l.Id, (r, l) => new { r, l })
                .Where(u => u.r.u.Ativo == 1)
                .AsQueryable();

            if (nome != null)
            {
                query = query.Where(a => a.r.u.Nome.ToLower().Contains(nome.ToLower()));
            }

            if (setor != null && setor != "0") 
            {
                query = query.Where(a => a.r.u.Setor.Id == int.Parse(setor));

            }
            if (loja != null && loja != "0")
            {
                query = query.Where(a => a.r.u.Loja.Id == int.Parse(loja));
            }

            var relat = query.Select(s => new
            {
                s.r.u.Id,
                s.r.u.Login,
                s.r.u.Nome,
                Setor = s.r.o.Nome,
                s.r.u.Perfil,
                s.r.u.Ativo,
                s.r.u.UltimoLogin,
                Loja = s.r.u.Loja.Nome

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
                    UltimoAcesso = user.UltimoLogin,
                    Loja = user.Loja

                };
                usuariosVM.Add(userVM);
            }

            ViewBag.PesquisaSetor = setor;
            ViewBag.PesquisaNome = nome;
            ViewBag.PesquisaLoja = loja;

            IPagedList<UsuariosViewModel> resultadoPaginado = usuariosVM.ToPagedList(pageNumber, 10);

            return View("Index", resultadoPaginado);

        }

        [HttpGet]
        public IActionResult Index(int? page)
        {
            ViewBag.Setores = setores;
            ViewBag.Lojas = lojas;
            int pageNumber = page ?? 0;


            var relat = _db.Int_DP_Usuarios
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
                    s.a.UltimoLogin,
                    Loja = s.a.Loja.Nome

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
                    UltimoAcesso = user.UltimoLogin,
                    Loja = user.Loja
                };
                usuariosVM.Add(userVM);
            }

            IPagedList<UsuariosViewModel> resultadoPaginado = usuariosVM.ToPagedList(pageNumber, 10);

            return View(resultadoPaginado);

        }

        [HttpGet]
        public ActionResult Excluir(int id)
        {
            Log log = new Log();
            int user_id = HttpContext.Session.GetInt32("ID") ?? 0;

            try
            {

                Usuario usuario = _db.Int_DP_Usuarios.Find(id);
                string usuario_temp = usuario.Login;
                usuario.Ativo = 0;
                usuario.Login = string.Concat(usuario.Login, Globalization.HoraAtualBR().Day.ToString(), Globalization.HoraAtualBR().Second.ToString(), Globalization.HoraAtualBR().Minute.ToString());
                //_db.Int_Dp_Usuarios.Remove(usuario);
                _db.SaveChanges();
                TempData["UsuarioExcluido"] = "O usuário '" + usuario_temp + "' foi excluido!";

                log.ExcluirUsuario(user_id, id);
                _db.Int_DP_Logs.Add(log);

            }
            catch (Exception exp)
            {

                TempData["UsuarioErro"] = "Ocorreu um erro ao tentar excluir o usuário";

                log.ExcluirUsuario_Erro(user_id, id, exp);
                _db.Int_DP_Logs.Add(log);

            }
            finally
            {
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Atualizar(int id)
        {
            Usuario usuario = _db.Int_DP_Usuarios.Find(id);

            usuario.Login = usuario.Login.ToLower();

            ViewBag.User = usuario;
            ViewBag.ConfirmaSenha = usuario.Senha;
            ViewBag.Setores2 = setores2;
            ViewBag.Lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();

            return View("Cadastrar");
        }

        [HttpPost]
        public ActionResult Atualizar([FromForm]Usuario usuario, string confirmasenha)
        {
            int user_id = HttpContext.Session.GetInt32("ID") ?? 0;
            Setor vSetor = _db.Int_DP_Setores.Find(usuario.Setor.Id);
            usuario.Setor = vSetor;

            Loja vLoja = _db.Int_DP_Lojas.Find(usuario.Loja.Id);
            usuario.Loja = vLoja;

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;
            ViewBag.Lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();

            if (ModelState.IsValid)
            {
                Usuario vUsuario = _db.Int_DP_Usuarios.Find(usuario.Id);
                if (vUsuario.Login.ToLower() != usuario.Login.ToLower()) //Se mudou o Login do Usuário
                {
                    Usuario vUsuario2 = _db.Int_DP_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault(); //Procura no banco para ver alguém já tem esse login

                    if (vUsuario2 == null)
                    {
                        usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                        usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Senha
                        confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Confirmação da Senha
                        if (usuario.Email != null)
                        {
                            usuario.Email = usuario.Email.ToLower();
                        }
                        else
                        {
                            usuario.Email = "";
                        }

                        if (usuario.Senha == confirmasenha)
                        {

                            Usuario vUpdate = _db.Int_DP_Usuarios.Find(usuario.Id);

                            Log log = new Log();
                            bool perfil, email;

                            if (usuario.Email != null && usuario.Email != "")
                            {

                                if (vUpdate.Email.ToLower() == usuario.Email.ToLower())
                                {
                                    email = true;
                                }
                                else
                                {
                                    var vEmail = _db.Int_DP_Usuarios.Where(a => a.Email == usuario.Email.ToLower() && a.Ativo == 1).FirstOrDefault();

                                    if (vEmail == null)
                                    {
                                        email = true;
                                    }
                                    else
                                    {
                                        email = false;
                                    }
                                }
                            }
                            else
                            {
                                email = true;
                                usuario.Email = "";
                            }

                            if (vUpdate.Perfil != usuario.Perfil)
                            {
                                perfil = true;
                            }
                            else
                            {
                                perfil = false;
                            }

                            if (email)
                            {

                                vUpdate.Login = usuario.Login;
                                vUpdate.Nome = usuario.Nome;
                                vUpdate.Perfil = usuario.Perfil;
                                vUpdate.Senha = usuario.Senha;
                                vUpdate.Setor = usuario.Setor;
                                vUpdate.Ativo = usuario.Ativo;
                                vUpdate.Email = usuario.Email;
                                vUpdate.Loja = usuario.Loja;

                                try
                                {

                                    _db.SaveChanges();
                                    TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi atualizado com sucesso!";

                                    log.AlterarUsuario(user_id, usuario.Id, perfil, usuario.Perfil);
                                    _db.Int_DP_Logs.Add(log);

                                }
                                catch (Exception exp)
                                {
                                    log.AlterarUsuario_Erro(user_id, usuario.Id, perfil, usuario.Perfil, exp);
                                    _db.Int_DP_Logs.Add(log);

                                    TempData["UsuarioErro"] = "Ocorreu um erro ao tentar alterar o usuário!";
                                }
                                finally
                                {
                                    _db.SaveChanges();
                                }

                                return RedirectToAction("Index");

                            }
                            else
                            {
                                TempData["ExisteUsuario"] = "Já existe um usuário com esse email cadastrado!";
                                ViewBag.User = usuario;
                                ViewBag.ConfirmaSenha = usuario.Senha;
                                return View("Cadastrar");
                            }
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
                        ViewBag.ConfirmaSenha = usuario.Senha;
                        return View("Cadastrar");
                    }

                }
                else
                {
                    usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                    usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Senha
                    confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Confirmação da Senha

                    if (usuario.Senha == confirmasenha)
                    {
                        bool perfil, email;

                        //Usuario vUpdate = _db.Int_DP_Usuarios.Find(usuario.Id)


                        Usuario vUpdate = _db.Int_DP_Usuarios.Where(a => a.Id == usuario.Id).FirstOrDefault();
                        
                        if (usuario.Email != null && usuario.Email != "")
                        {

                            if (vUpdate.Email.ToLower() == usuario.Email.ToLower())
                            {
                                email = true;
                            }
                            else
                            {
                                var vEmail = _db.Int_DP_Usuarios.Where(a => a.Email.ToLower() == usuario.Email.ToLower() && a.Ativo == 1).FirstOrDefault();

                                if (vEmail == null)
                                {
                                    email = true;
                                }
                                else
                                {
                                    email = false;
                                }
                            }
                        }
                        else
                        {
                            email = true;
                            usuario.Email = "";
                        }

                        if (vUpdate.Perfil != usuario.Perfil)
                        {
                            perfil = true;
                        }
                        else
                        {
                            perfil = false;
                        }

                        if (email)
                        {

                            vUpdate.Login = usuario.Login;
                            vUpdate.Nome = usuario.Nome;
                            vUpdate.Perfil = usuario.Perfil;
                            vUpdate.Senha = usuario.Senha;
                            vUpdate.Setor = usuario.Setor;
                            vUpdate.Ativo = usuario.Ativo;
                            vUpdate.Email = usuario.Email;
                            vUpdate.Loja = usuario.Loja;

                            Log log = new Log();


                            try
                            {
                                _db.SaveChanges();
                                TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi atualizado com sucesso!";

                                log.AlterarUsuario(user_id, usuario.Id, perfil, usuario.Perfil);
                                _db.Int_DP_Logs.Add(log);

                            }
                            catch (Exception exp)
                            {

                                log.AlterarUsuario_Erro(user_id, usuario.Id, perfil, usuario.Perfil, exp);
                                _db.Int_DP_Logs.Add(log);
                                TempData["UsuarioErro"] = "Erro ao tentar atualizar o usuário!";

                            }
                            finally
                            {
                                _db.SaveChanges();
                            }


                            return RedirectToAction("Index");

                        }
                        else
                        {
                            TempData["ExisteUsuario"] = "Já existe um usuário com esse login, favor escolher outro!";
                            ViewBag.User = usuario;
                            ViewBag.ConfirmaSenha = usuario.Senha;
                            return View("Cadastrar");
                        }
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
            ViewBag.Lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar([FromForm]Usuario usuario, string confirmasenha)
        {
            Setor vSetor = _db.Int_DP_Setores.Find(usuario.Setor.Id);
            usuario.Setor = vSetor;

            Loja vLoja = _db.Int_DP_Lojas.Find(usuario.Loja.Id);
            usuario.Loja = vLoja;

            ViewBag.User = new Usuario();
            ViewBag.Setores2 = setores2;
            ViewBag.Lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();

            if (ModelState.IsValid)
            {
                Usuario vUsuario = _db.Int_DP_Usuarios.Where(a => a.Login.Equals(usuario.Login)).FirstOrDefault();

                if (vUsuario == null)
                {
                    usuario.Login = usuario.Login.ToLower(); //Passa para minúsculo o Login
                    usuario.Senha = usuario.Senha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Senha
                    confirmasenha = confirmasenha.Replace(";", "").Replace(",", "").Replace(".", "").Replace("'", "").ToLower(); //Passa para minúsculo a Confirmação da Senha
                    if (usuario.Email != null)
                    {
                        usuario.Email = usuario.Email.ToLower();
                    }
                    else
                    {
                        usuario.Email = "";
                    }
                    usuario.UltimoLogin = Globalization.HoraAtualBR();
                    usuario.DataCadastro = Globalization.HoraAtualBR();

                    if (usuario.Senha == confirmasenha)
                    {

                        Usuario vEmail = new Usuario();

                        if (usuario.Email != null && usuario.Email != "")
                        {

                            vEmail = _db.Int_DP_Usuarios.Where(a => a.Email == usuario.Email && a.Ativo == 1).FirstOrDefault();

                        }
                        else
                        {
                            vEmail = null;
                        }

                        if (vEmail == null)
                        {
                            int id_user = HttpContext.Session.GetInt32("ID") ?? 0;

                            try
                            {
                                _db.Int_DP_Usuarios.Add(usuario);
                                _db.SaveChanges();

                                Log log = new Log();
                                log.CadastrarUsuario(id_user, usuario.Id);
                                _db.Int_DP_Logs.Add(log);

                                TempData["CadastroUserOK"] = "O usuário '" + usuario.Login + "' foi cadastrado com sucesso!";

                            }
                            catch (Exception exp)
                            {
                                Log log = new Log();
                                log.CadastrarUsuario_Erro(id_user, usuario.Login, exp);
                                _db.Int_DP_Logs.Add(log);
                                TempData["CadastroUserNotOK"] = "Erro ao cadastrar o usuário!";
                            }
                            finally
                            {
                                _db.SaveChanges();
                            }

                            return RedirectToAction("Index");

                        }
                        else
                        {

                            TempData["ExisteUsuario"] = "Já existe um usuário com esse email cadastrado!";
                            ViewBag.User = usuario;
                            return View();
                        }

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