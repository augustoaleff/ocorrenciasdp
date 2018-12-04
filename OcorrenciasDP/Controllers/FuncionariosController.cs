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

        public FuncionariosController(DatabaseContext db)
        {
            _db = db;
        }
        
        public IActionResult Index(int? page)
        {

            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Setores = setores;

            int pageNumber = page ?? 1;

            var relat = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Usuarios, a => a.Encarregado.Id, b => b.Id, (a, b) => new { a, b })
                        .Where(u => u.a.Ativo == 1)
                        .OrderBy(o => o.a.Nome)
                        .OrderBy(o => o.a.Id)
                        .Select(s => new
                        {
                            s.a.Nome,
                            s.a.Id,
                            s.a.Experiencia,
                            Encarregado = s.b.Nome
                        })
                        .ToList();

            List<FuncionarioViewModel> relatVM = new List<FuncionarioViewModel>();

            foreach (var func in relat)
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
            return View(resultadoPaginado);

        }

        [HttpGet]
        public ActionResult Cadastrar()
        {
            ViewBag.Func = new Funcionario();
            ViewBag.Setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();

            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar([FromForm]Funcionario func, bool experiencia, int exp_periodo)
        {
            ViewBag.Func = func;
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 1;

            //Verifica se há um funcionário com o mesmo nome e encarregado cadastrado
            Funcionario vFuncionario = _db.Int_DP_Funcionarios
                                       .Where(a => a.Nome == func.Nome && a.Encarregado.Id == func.Encarregado.Id)
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

                    
                    TempData["FuncionarioOK"] = "Funcionário cadastrado com sucesso!";
                    log.CadastrarFuncionario(id_notnull, func.Id);
                    _db.SaveChanges();

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

            ViewBag.Setores = _db.Int_DP_Setores
                              .OrderBy(a => a.Nome)
                              .ToList();

            
            Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);

            ViewBag.Func = func;

            ViewBag.Setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            ViewBag.Encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();
            
            return View("Cadastrar");
        }

        public ActionResult Filtrar(string nome, int? setor, int? encarregado, int? page)
        {
            int pageNumber = page ?? 1;

            ViewBag.Setores = _db.Int_DP_Setores
                              .OrderBy(a => a.Nome)
                              .ToList();

            ViewBag.Setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();

            ViewBag.Encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();


            var query = _db.Int_DP_Funcionarios
                         .AsQueryable();

            if(nome != null)
            {
                query.Where(a => a.Nome.ToLower() == nome.ToLower());
            }

            if(setor != null)
            {
                query.Where(a => a.Setor.Id == setor);
            }

            if(encarregado != null)
            {
                query.Where(a => a.Encarregado.Id == encarregado);
            }

            List<Funcionario> funcionarios = query.ToList();
            List<FuncionarioViewModel> funcionariosVM = new List<FuncionarioViewModel>();

            foreach(Funcionario func in funcionarios)
            {

                FuncionarioViewModel funcVM = new FuncionarioViewModel
                {
                    Id = func.Id,
                    Nome = func.Nome,
                    Encarregado = func.Encarregado.Nome,
                    Experiencia = func.Experiencia
                };

                funcionariosVM.Add(funcVM);
            }

            IPagedList<FuncionarioViewModel> resultadoPaginado = funcionariosVM.ToPagedList(pageNumber, 10);

            return View("Index",resultadoPaginado);
            
        }

        [HttpPost]
        public ActionResult Atualizar([FromForm]Funcionario func)
        {
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            ViewBag.Setores = _db.Int_DP_Setores
                              .OrderBy(a => a.Nome)
                              .ToList();
            
            ViewBag.Func = func;

            try
            {
                Funcionario func_antigo = _db.Int_DP_Funcionarios.Find(func.Id);

                func_antigo.Nome = func.Nome;
                func_antigo.Setor = func.Setor;
                func_antigo.Encarregado = func.Encarregado;
                func_antigo.Experiencia = func.Experiencia;
                func_antigo.Exp_DataFim = func.Exp_DataFim;
                func_antigo.Exp_DataInicio = func.Exp_DataFim;

                /*func_antigo.Ferias_DataInicio = func.Ferias_DataInicio;
                func_antigo.Ferias_DataLimite = func.Ferias_DataLimite;
                func_antigo.Ferias_Periodo = func.Ferias_Periodo;*/

                _db.SaveChanges();
                TempData["FuncionarioNotOK"] = "Funcionário Atualizado com Sucesso";
                
            }
            catch (Exception exp)
            {

                Log log = new Log();
                log.AlterarFuncionario_Erro(id_notnull, func.Id, exp);
                _db.SaveChanges();

                TempData["FuncionarioNotOK"] = "Ocorreu um erro ao tentar atualizar o funcionário";

            }

            return RedirectToAction("Index");

        }
    }
}
