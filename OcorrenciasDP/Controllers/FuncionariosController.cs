using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private List<Setor> setores;
        private List<Usuario> encarregados;
        private List<Setor> setores2;
        private List<Loja> lojas;


        public FuncionariosController(DatabaseContext db)
        {
            _db = db;

            setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();


            setores = setores2;
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });

            lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();
            lojas.Add(new Loja() { Id = 0, Nome = "*Todas*" });
            
            encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

        }
        
        public IActionResult Index(int? page)
        {

            //setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            //ViewBag.Setores = setores;

            int pageNumber = page ?? 1;

            var relat = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Lojas, a => a.Loja.Id, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Int_DP_Setores, c => c.a.Setor.Id, d => d.Id, (c, d) => new { c, d })
                        .Where(u => u.c.a.Ativo == 1)
                        .OrderBy(o => o.c.a.Nome)
                        .OrderBy(o => o.c.a.Id)
                        .Select(s => new
                        {
                            s.c.a.Nome,
                            s.c.a.Id,
                            s.c.a.Experiencia,
                            Setor = s.d.Nome,
                            Loja = s.c.b.Nome
                        })
                        .ToList();

            List<FuncionarioViewModel> relatVM = new List<FuncionarioViewModel>();

            foreach (var func in relat)
            {
                FuncionarioViewModel funcVM = new FuncionarioViewModel
                {
                    Id = func.Id,
                    Nome = func.Nome,
                    Experiencia = func.Experiencia,
                    Setor = func.Setor,
                    Loja = func.Loja
                    
                };

                relatVM.Add(funcVM);

            }

            IPagedList<FuncionarioViewModel> resultadoPaginado = relatVM.ToPagedList(pageNumber, 10);
            return View(resultadoPaginado);

        }

        
        public ActionResult Cadastrar()
        {
            ViewBag.Func = new Funcionario();

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            return View();
        }
            
        [HttpPost]
        public ActionResult Cadastrar([FromForm]Funcionario func, bool experiencia, int exp_periodo)
        {
            ViewBag.Func = new Funcionario();
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 1;

            //Verifica se há um funcionário com o mesmo nome e encarregado cadastrado
            Funcionario vFuncionario = _db.Int_DP_Funcionarios
                                       .Where(a => a.Nome.Equals(func.Nome) && a.Encarregado.Id.Equals(func.Encarregado.Id))
                                       .FirstOrDefault();
            
            if (vFuncionario == null)
            {
                Log log = new Log();

                try
                {  
                    func.DataCadastro = Globalization.HoraAtualBR();
                    func.Encarregado = _db.Int_DP_Usuarios.Find(func.Encarregado.Id);
                    func.Setor = _db.Int_DP_Setores.Find(func.Setor.Id);
                    int id_user = HttpContext.Session.GetInt32("ID") ?? 0;
                    func.CadastradoPor = _db.Int_DP_Usuarios.Find(id_user);
                    func.Exp_DataFim = func.Exp_DataInicio.AddDays(exp_periodo);

                    if (experiencia)
                    {
                        func.Experiencia = 1;
                    }
                    else
                    {
                        func.Experiencia = 0;
                    }

                    _db.Int_DP_Funcionarios.Add(func);
                    _db.SaveChanges();
                    
                    log.CadastrarFuncionario(id_notnull, func.Id);
                    _db.SaveChanges();

                    TempData["FuncionarioOK"] = "Funcionário cadastrado com sucesso!";

                    return RedirectToAction("Cadastrar");

                }
                catch (Exception exp)
                {
                    log.CadastrarFuncionario_Erro(id_notnull, func.Id, exp);
                    _db.SaveChanges();

                    TempData["FuncionarioNotOK"] = "Ocorreu um erro ao tentar inserir o funcionário!";

                    return View();

                }
            }
            else
            {
                TempData["FuncionarioNotOK"] = "Já existe um funcionário cadastrado com esse Nome e Encarregado";
            }

            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult Excluir(long? id)
        {
            long id_notnull = id ?? 0;

            Funcionario vFuncionario = _db.Int_DP_Funcionarios.Find(id_notnull);
            vFuncionario.Ativo = 0;

            _db.SaveChanges();

            TempData["FuncionarioNotOK"] = "Funcionário Excluido";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Atualizar(long? id)
        {
            long id_notnull = id ?? 0;

            /*ViewBag.Setores = _db.Int_DP_Setores
                              .OrderBy(a => a.Nome)
                              .ToList();*/
           
            Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);

            ViewBag.Func = func;

            /*ViewBag.Setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();*/

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            return View("Cadastrar");

        }

        public ActionResult Filtrar(string nome, int?loja, int? setor, int? page)
        {

            var relat = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Lojas, a => a.Loja.Id, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Int_DP_Setores, c => c.a.Setor.Id, d => d.Id, (c, d) => new { c, d })
                        .Where(u => u.c.a.Ativo == 1)
                        .OrderBy(o => o.c.a.Nome)
                        .OrderBy(o => o.c.a.Id)
                        .Select(s => new
                        {
                            s.c.a.Nome,
                            s.c.a.Id,
                            s.c.a.Experiencia,
                            Setor = s.d.Nome,
                            Loja = s.c.b.Nome
                        })
                        .ToList();

            int pageNumber = page ?? 1;
            
            var query = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Lojas, a => a.Loja.Id, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Int_DP_Setores, c => c.a.Setor.Id, d => d.Id, (c, d) => new { c, d })
                        .Where(u => u.c.a.Ativo == 1)
                        .OrderBy(o => o.c.a.Nome)
                        .OrderBy(o => o.c.a.Id)
                        .AsQueryable();

            if(nome != null)
            {
                query.Where(a => a.c.a.Nome.ToLower() == nome.ToLower());
            }

            if(setor != null)
            {
                query.Where(a => a.c.a.Setor.Id == setor);
            }

            if (loja != null)
            {
                query.Where(a => a.c.a.Loja.Id == loja);
            }

            var funcionarios = query.ToList();
            List<FuncionarioViewModel> funcionariosVM = new List<FuncionarioViewModel>();

            foreach(var func in funcionarios)
            {
               
                FuncionarioViewModel funcVM = new FuncionarioViewModel
                {
                    Id = func.c.a.Id,
                    Nome = func.c.a.Nome,
                    Experiencia = func.c.a.Experiencia,
                    Setor = func.d.Nome,
                    Loja = func.c.b.Nome,

                };

                funcionariosVM.Add(funcVM);
            }

            IPagedList<FuncionarioViewModel> resultadoPaginado = funcionariosVM.ToPagedList(pageNumber, 10);
            
            return View("Index",resultadoPaginado);

        }

        [HttpPost]
        public ActionResult Atualizar([FromForm]Funcionario func, bool experiencia, int exp_periodo)
        {
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;
            
           /* ViewBag.Setores = _db.Int_DP_Setores
                              .OrderBy(a => a.Nome)
                              .ToList();*/
            
            ViewBag.Func = func;

            Log log = new Log();

            try
            {
                func.Setor = _db.Int_DP_Setores.Find(func.Setor.Id);
                func.Encarregado = _db.Int_DP_Usuarios.Find(func.Encarregado.Id);

                Funcionario func_antigo = _db.Int_DP_Funcionarios.Find(func.Id);
                
                func_antigo.Setor = func.Setor;
                func_antigo.Encarregado = func.Encarregado;
                func_antigo.Exp_DataInicio = func.Exp_DataInicio;
                func_antigo.Exp_DataFim = func.Exp_DataInicio.AddDays(exp_periodo);
                
                if (experiencia)
                {
                    func_antigo.Experiencia = 1;
                }
                else
                {
                    func_antigo.Experiencia = 0;
                }
                
                //Férias
                /*func_antigo.Ferias_DataInicio = func.Ferias_DataInicio;
                func_antigo.Ferias_DataLimite = func.Ferias_DataLimite;
                func_antigo.Ferias_Periodo = func.Ferias_Periodo;*/

                _db.SaveChanges();
                log.AlterarFuncionario(id_notnull, func.Id);
                
                TempData["FuncionarioNotOK"] = "Funcionário Atualizado com Sucesso";

            }
            catch (Exception exp)
            {  
                log.AlterarFuncionario_Erro(id_notnull, func.Id, exp);

                TempData["FuncionarioNotOK"] = "Ocorreu um erro ao tentar atualizar o funcionário";

            }
            finally{
                
                _db.Int_DP_Logs.Add(log);
                _db.SaveChanges();

            }

            return RedirectToAction("Index");
        }
    }
}
