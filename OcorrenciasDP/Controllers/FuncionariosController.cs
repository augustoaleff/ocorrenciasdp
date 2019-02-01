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
using Rotativa.AspNetCore;
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
            DeclararViewBags();  //Função para estabelecer as ViewBags Necessárias para as abrir as Views
        }

        public void DeclararViewBags()
        {
            List<Setor> setores;
            List<Usuario> encarregados;
            List<Setor> setores2;
            List<Loja> lojas;
            List<Loja> lojas2;
            
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
            ViewBag.Lojas = lojas; //Lojas com "*todos*"
            ViewBag.Lojas2 = lojas2; //Lojas sem "*todos*"
            ViewBag.Encarregados = encarregados; //Encarregados sem "*todos*"
        }

        public void AtualizarExperiencia()
        {
            List<Funcionario> func_exp = _db.Int_DP_Funcionarios.ToList();

            DateTime hoje = Globalization.HojeBR();

            foreach (var func in func_exp)
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
        }

        public IActionResult Index(int? page)
        {
            DeclararViewBags();
            
            int pageNumber = page ?? 1;

            AtualizarExperiencia();

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

            DeclararViewBags();

            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar([FromForm]Funcionario func, int exp_periodo)
        {
            DeclararViewBags();

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

            DeclararViewBags();

            return View("Cadastrar");

        }

        [HttpGet]
        public ActionResult Filtrar(string nome, int? loja, int? setor, int? page)
        {
            DeclararViewBags();

            AtualizarExperiencia();

            int pageNumber = page ?? 1;

            var query = _db.Int_DP_Funcionarios
                        .Join(_db.Int_DP_Lojas, a => a.Loja.Id, b => b.Id, (a, b) => new { a, b })
                        .Join(_db.Int_DP_Setores, c => c.a.Setor.Id, d => d.Id, (c, d) => new { c, d })
                        .Where(u => u.c.a.Ativo == 1)
                        .AsQueryable();

            string nome1 = nome;
            int loja1 = loja ?? 0;
            int setor1 = setor ?? 0;
            int page1 = page ?? 0;


            if (nome1 != null)
            {
                query = query.Where(a => a.c.a.Nome.ToLower().Contains(nome1.ToLower()));
            }

            if (setor1 != 0)
            {
                query = query.Where(a => a.c.a.Setor.Id == setor1);
            }
            if (loja1 != 0)
            {
                query = query.Where(a => a.c.a.Loja.Id == loja1);
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

            ViewBag.PesquisaSetor = setor1;
            ViewBag.PesquisaNome = nome1;
            ViewBag.PesquisaLoja = loja1;
            
            IPagedList<FuncionarioViewModel> resultadoPaginado = funcionariosVM.ToPagedList(pageNumber, 10);

            return View("Index", resultadoPaginado);

        }

        public ActionResult RHO_EnviarArquivo()
        {
            return View();
        }

        //Incluir novo funcionário no RHOnline
        public ActionResult RHO_Incluir()
        {
            DeclararViewBags();

            ViewBag.RHOFunc = new RHO_Usuario();
    
            return View();
        }

        [HttpPost]
        public ActionResult RHO_Incluir([FromForm]RHO_Usuario usuario)
        {
            DeclararViewBags();

            ViewBag.RHOFunc = usuario;

            usuario.CPF = usuario.CPF.Replace(".", "").Replace("-", "").Replace("/", "").Replace("\\", "").Replace(",", "").Trim();

            RHO_Usuario vUsuario = _db.Int_RH_Usuarios.Where(a => a.CPF.Equals(usuario.CPF)).FirstOrDefault();

            if (vUsuario == null)
            {
                usuario.DataCadastro = Globalization.HoraAtualBR();
                usuario.UltimoAcesso = Globalization.HoraAtualBR();
                usuario.Nivel = 2;
                usuario.Ativo = 1;
                usuario.Cadastrado = 0;

                ViewBag.RHOFunc = usuario;

                if (usuario.CPF.Length == 11)
                {
                    try
                    {
                        _db.Int_RH_Usuarios.Add(usuario);

                        _db.SaveChanges();

                        TempData["RHO_IncluirOK"] = "Inclusão efetuada com sucesso, código: " + usuario.CodigoAtivacao;

                        return RedirectToAction("RHO_Incluir");

                    }
                    catch (Exception exp)
                    {
                        TempData["RHO_IncluirNotOK"] = "Ocorreu um Erro ao tentar incluir o funcionário";
                        return View("RHO_Incluir");
                    }
                }
                else
                {
                    TempData["RHO_IncluirNotOK"] = "CPF não valido";

                    return View("RHO_Incluir");
                }
            }
            else
            {
                TempData["RHO_IncluirNotOK"] = "CPF já cadastrado no sistema!";
                return View("RHO_Incluir");
            }
        }

        [HttpGet]
        public ActionResult Detalhar(long? id)
        {

            long id_notnull = id ?? 0;

            int user_id = HttpContext.Session.GetInt32("ID") ?? 0;

            int periodo = 0;

            Log log = new Log();

            try
            {
                Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);
                
                if (func.Experiencia == 1)
                {
                    periodo = func.Exp_DataFim.Subtract(func.Exp_DataInicio).Days;
                }

                ViewBag.DetalharFunc = func;
                ViewBag.Exp_Periodo = periodo;

                List<Avaliacao> avaliacao = _db.Int_DP_Avaliacoes
                    .Where(a => a.Funcionario.Id == id_notnull)
                    .OrderBy(o => o.DataAvaliacao)
                    .ToList();

                foreach (var nota in avaliacao)
                {
                    int encarregado = _db.Int_DP_Avaliacoes.Where(a => a.Id == nota.Id).Select(s => s.Encarregado.Id).FirstOrDefault();

                    nota.Encarregado = _db.Int_DP_Usuarios.Find(encarregado);
                    nota.Funcionario = _db.Int_DP_Funcionarios.Find(nota.Funcionario.Id);

                }

                ViewBag.FuncNotas = avaliacao;
                log.VisualizacaoDetalhe(user_id, id_notnull);

            }

            catch (Exception exp)
            {
                log.VisualizacaoDetalhe_Erro(user_id, id_notnull, exp);
            }
            finally
            {
                _db.SaveChanges();
            }

            return View();
        }

        public IActionResult ImprimirDetalhes(long? id)
        {
            long id_notnull = id ?? 0;

            int user_id = HttpContext.Session.GetInt32("ID") ?? 0;

            string data = Globalization.DataRelatorioPdfBR();

            Funcionario func = _db.Int_DP_Funcionarios.Find(id_notnull);

            func.Loja = _db.Int_DP_Lojas.Where(a => a.Id == func.Loja.Id).FirstOrDefault();
            func.Setor = _db.Int_DP_Setores.Where(a => a.Id == func.Setor.Id).FirstOrDefault();

            List<Avaliacao> avaliacoes = _db.Int_DP_Avaliacoes
                .Where(a => a.Funcionario.Id == func.Id)
                .OrderByDescending(o => o.DataAvaliacao)
                .ToList(); 


            DetalhesFuncViewModel detalhesVM = new DetalhesFuncViewModel
            {
                Funcionario = func,
                Usuario = _db.Int_DP_Usuarios.Find(user_id),
                Avaliacoes = avaliacoes,
                Data = Globalization.HoraAtualBR()
            };

            ViewAsPdf relatorioPDF = new ViewAsPdf
            {
                WkhtmlPath = "~/OcorrenciasDP/wwwroot/Rotativa",
                ViewName = "DetalhesEmPDF",
                IsGrayScale = false,
                Model = detalhesVM,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--page-offset 0 --footer-left " + data + " --footer-right [page]/[toPage] --footer-font-size 8",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };

            return relatorioPDF;
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
