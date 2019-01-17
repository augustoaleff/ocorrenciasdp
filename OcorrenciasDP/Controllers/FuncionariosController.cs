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
        private readonly List<Setor> setores;
        private readonly List<Usuario> encarregados;
        private readonly List<Setor> setores2;
        private readonly List<Loja> lojas;
        private readonly List<Loja> lojas2;


        public FuncionariosController(DatabaseContext db)
        {
            _db = db;

            setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            setores2 = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            setores.Add(new Setor() { Id = 0, Nome = "*Todos*" });

            lojas = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();
            lojas2 = _db.Int_DP_Lojas.OrderBy(a => a.Id).ToList();
            lojas.Add(new Loja() { Id = 0, Nome = "*Todas*" });


            encarregados = _db.Int_DP_Usuarios
                                   .Where(a => a.Perfil.Equals("usuario") && a.Ativo == 1)
                                   .OrderBy(a => a.Nome)
                                   .ToList();

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

        }

        public IActionResult Index(int? page)
        {
            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            //setores = _db.Int_DP_Setores.OrderBy(a => a.Nome).ToList();
            //ViewBag.Setores = setores;
            
            int pageNumber = page ?? 1;

            var func__exp = _db.Int_DP_Funcionarios.ToList();

            DateTime hoje = Globalization.HojeBR();

            foreach (var func in func__exp)
            {
                if (func.Exp_DataInicio.Date <= hoje & func.Exp_DataFim.Date >= hoje)
                {
                    func.Experiencia = 1;
                }
                else
                {
                    func.Experiencia = 0;
                }

                _db.SaveChanges();
            }

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
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar([FromForm]Funcionario func, int exp_periodo)
        {

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            ViewBag.Func = new Funcionario();
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 1;

            DateTime hoje = Globalization.HojeBR();

            //Verifica se há um funcionário com o mesmo nome e encarregado cadastrado
            Funcionario vFuncionario = _db.Int_DP_Funcionarios
                                       .Where(a => a.Nome.Equals(func.Nome) && a.Setor.Id == func.Setor.Id &&
                                       a.Loja.Id == func.Loja.Id && a.Ativo == 1)
                                       .FirstOrDefault();

            if (vFuncionario == null)
            {
                Log log = new Log();

                try
                {
                    func.DataCadastro = Globalization.HoraAtualBR();
                    func.Setor = _db.Int_DP_Setores.Find(func.Setor.Id);
                    func.Loja = _db.Int_DP_Lojas.Find(func.Loja.Id);
                    int id_user = HttpContext.Session.GetInt32("ID") ?? 0;
                    func.CadastradoPor = _db.Int_DP_Usuarios.Find(id_user);
                    func.Exp_DataFim = func.Exp_DataInicio.AddDays(exp_periodo);



                    if (exp_periodo != 0)
                    {

                        if (func.Exp_DataFim >= hoje && func.Exp_DataInicio <= hoje)
                        {
                            func.Experiencia = 1;
                        }
                        else
                        {
                            func.Experiencia = 0;
                        }

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

                    return RedirectToAction("Index");

                }
                catch (Exception exp)
                {
                    ViewBag.Func = func;

                    log.CadastrarFuncionario_Erro(id_notnull, func.Id, exp);
                    _db.SaveChanges();

                    TempData["FuncionarioNotOK"] = "Ocorreu um erro ao tentar inserir o funcionário!";

                    return View();

                }
            }
            else
            {
                if (exp_periodo != 0)
                {
                    func.Experiencia = 1;
                }
                else
                {
                    func.Experiencia = 0;
                }

                ViewBag.Func = func;

                TempData["FuncionarioNotOK"] = "Já existe um funcionário cadastrado com esse Nome e Setor";
            }

            return View();

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

            Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);

            ViewBag.Func = func;

            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            return View("Cadastrar");

        }

        [HttpGet]
        public ActionResult Filtrar(string nome, string loja, string setor, int? page)
        {
            ViewBag.Setores = setores;  //Setores com "*todos*"
            ViewBag.Setores2 = setores2; //Setores sem "*todos*"
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"

            int pageNumber = page ?? 1;

            var query = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Lojas, a => a.Loja.Id, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Int_DP_Setores, c => c.a.Setor.Id, d => d.Id, (c, d) => new { c, d })
                        .Where(u => u.c.a.Ativo == 1)
                        .AsQueryable();

            if (nome != null)
            {
                query.Where(a => a.c.a.Nome.ToLower().Equals(nome.ToLower()));
            }

            if (setor != null)
            {
                if (setor != "" && setor != "0")
                {
                    query.Where(a => a.c.a.Setor.Id == int.Parse(setor));
                }
            }

            if (loja != null)
            {
                if (loja != "" && loja != "0")
                {
                    query.Where(a => a.c.a.Loja.Id == int.Parse(loja));
                }
            }

            var relat = query.Select(s => new
            {
                s.c.a.Id,
                s.c.a.Nome,
                s.c.a.Experiencia,
                Setor = s.d.Nome,
                Loja = s.c.b.Nome
            }).ToList();

            List<FuncionarioViewModel> funcionariosVM = new List<FuncionarioViewModel>();

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

                funcionariosVM.Add(funcVM);
            }

            ViewBag.PesquisaSetor = setor;
            ViewBag.PesquisaNome = nome;
            ViewBag.PesquisaLoja = loja;
            
            IPagedList<FuncionarioViewModel> resultadoPaginado = funcionariosVM.ToPagedList(pageNumber, 10);

            return View("Index", resultadoPaginado);

        }
        
        [HttpGet]
        public ActionResult Detalhar(long? id)
        {
            long id_notnull = id ?? 0;

            int periodo = 0;

            Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);

            if (func.Experiencia == 1)
            {
                periodo = func.Exp_DataFim.Subtract(func.Exp_DataInicio).Days;
            }
            
            ViewBag.DetalharFunc = func;
            ViewBag.Exp_Periodo = periodo;

            List<Avaliacao> avaliacao = _db.Int_DP_Avaliacoes
                .Where(a => a.Funcionario.Id == id_notnull)
                .OrderByDescending(o => o.DataAvaliacao)
                .ToList();

            foreach (var nota in avaliacao)
            {
                int encarregado = _db.Int_DP_Avaliacoes.Where(a => a.Id == nota.Id).Select(s => s.Encarregado.Id).FirstOrDefault();

                nota.Encarregado = _db.Int_DP_Usuarios.Find(encarregado);
                nota.Funcionario = _db.Int_DP_Funcionarios.Find(nota.Funcionario.Id);

            }

            ViewBag.FuncNotas = avaliacao;

            return View();
        }


        [HttpPost]
        public ActionResult Atualizar([FromForm]Funcionario func, int exp_periodo)
        {
            int id_notnull = HttpContext.Session.GetInt32("ID") ?? 0;

            ViewBag.Func = func;

            Log log = new Log();

            try
            {
                DateTime hoje = Globalization.HojeBR();

                func.Setor = _db.Int_DP_Setores.Find(func.Setor.Id);
                func.Loja = _db.Int_DP_Lojas.Find(func.Loja.Id);

                Funcionario func_antigo = _db.Int_DP_Funcionarios.Find(func.Id);

                func_antigo.Setor = func.Setor;
                func_antigo.Loja = func.Loja;
                func_antigo.Exp_DataInicio = func.Exp_DataInicio;
                func_antigo.Exp_DataFim = func.Exp_DataInicio.AddDays(exp_periodo);


                if (exp_periodo != 0)
                {

                    if (func_antigo.Exp_DataFim.Date >= hoje && func_antigo.Exp_DataInicio.Date <= hoje)
                    {
                        func_antigo.Experiencia = 1;
                    }
                    else
                    {
                        func_antigo.Experiencia = 0;
                    }

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

                TempData["FuncionarioOK"] = "Funcionário Atualizado com Sucesso";

            }
            catch (Exception exp)
            {
                log.AlterarFuncionario_Erro(id_notnull, func.Id, exp);

                TempData["FuncionarioNotOK"] = "Ocorreu um erro ao tentar atualizar o funcionário";

            }
            finally
            {

                _db.Int_DP_Logs.Add(log);
                _db.SaveChanges();

            }

            return RedirectToAction("Index");
        }
    }
}
