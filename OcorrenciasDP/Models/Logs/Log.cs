using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OcorrenciasDP.Library.Globalization;

namespace OcorrenciasDP.Models
{
    public class Log
    {
        public long Id { get; set; }
        public int Usuario { get; set; }
        public DateTime Data { get; set; }
        public int Tipo { get; set; }
        public long Ocorrencia { get; set; }
        public int UsuarioAlterado { get; set; }
        public long Mensagem { get; set; }
        public string Obs { get; set; }
        public long Funcionario { get; set; }
        public string Sistema_Operacional { get; set; }

        public Log()
        {
            this.Sistema_Operacional = Environment.OSVersion.VersionString;
            this.Data = Globalization.HoraAtualBR();
        }

        public void LogIn(int usuario)
        {
            this.Usuario = usuario;
            this.Tipo = 2; //Login efetuado com Sucesso 
        }

        public void LogIn_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Obs = "ERRO: " + e.Message;
            this.Tipo = 1; //Login efetuado com Erro 
        }

        public void LogOut(int usuario)
        {
            this.Usuario = usuario;
            this.Tipo = 4; //Logout efetuado com Sucesso
        }

        public void LogOut_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 3; //Logout efetuado com Sucesso
            this.Obs = "ERRO: " + e.Message;
        }

        public void CadastrarUsuario(int usuario, int usuarioCadastrado)
        {
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioCadastrado;
            this.Tipo = 302; //Criação de Usuario com Sucesso
        }

        public void CadastrarUsuario_Erro(int usuario, string loginUsuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 301; //Criação de Usuário com Erro
            this.Obs = "ERRO: " + e.Message + ", Login Usuário = " + loginUsuario;
        }

        public void ExcluirUsuario(int usuario, int usuarioExcluido)
        {
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioExcluido;
            this.Tipo = 304; //Exclusão de Usuario com Sucesso
        }

        public void ExcluirUsuario_Erro(int usuario, int usuarioExcluido, Exception e)
        {
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioExcluido;
            this.Tipo = 303; //Exclusão de Usuario com Erro
            this.Obs = "ERRO: " + e.Message;
        }

        public void AlterarUsuario(int usuario, int usuarioAlterado, bool alteraPerfil, string perfil)
        {
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioAlterado;

            if (alteraPerfil)
            {
                this.Tipo = 308; //Alteração de Permissão de Usuário com Sucesso
                this.Obs = "Alterado o perfil do usuário para: " + perfil;
            }
            else
            {
                this.Tipo = 306; //Alteração de usuário com Sucesso
            }

        }

        public void AlterarUsuario_Erro(int usuario, int usuarioAlterado, bool alteraPerfil, string perfil, Exception e)
        {

            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioAlterado;
            this.Obs = "ERRO: " + e.Message;

            if (alteraPerfil)
            {
                this.Tipo = 307; //Alteração de Permissão de Usuário com Erro
                this.Obs += ". Alterado o perfil do usuário para : " + perfil;
            }
            else
            {
                this.Tipo = 305; //Alteração de usuário com Erro
            }
        }

        public void IncluirOcorrencia(int usuario, long ocorrencia)
        {
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 202; //Envio de Ocorrencia com sucesso

        }

        public void IncluirOcorrencia_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 201; //Envio de Ocorrencia com erro
            this.Obs = "ERRO: " + e.Message;
        }

        public void ExcluirOcorrencia(int usuario, long ocorrencia)
        {
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 204; //Exclusão de Ocorrencia com sucesso
        }

        public void ExcluirOcorrencia_Erro(int usuario, long ocorrencia, Exception e)
        {
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 203; //Exclusão de Ocorrencia com erro
            this.Obs = "ERRO: " + e.Message;

        }

        public void EnviarLembrete(int usuario, int total)
        {
            this.Usuario = usuario;
            this.Tipo = 402; //Envio de Lembrete com Sucesso
            if (total == 1)
            {
                this.Obs = "Enviado 1 email com sucesso";
            }
            else
            {
                this.Obs = "Enviado " + total + " emails com sucesso";
            }
        }

        public void EnviarLembrete_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 401; //Envio de Lembrete com Erro
            this.Obs = "ERRO: " + e.Message;
        }

        public void EnviarMensagem(int usuario, long mensagem)
        {
            this.Usuario = usuario;
            this.Tipo = 404; //Envio de Mensagem com Sucesso
            this.Mensagem = mensagem;
        }

        public void EnviarMensagem_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 403; // Envio de Mensagem com Erro
            this.Obs = "ERRO: " + e.Message;
        }

        public void ExportarRelatorio(int usuario, string filtros)
        {
            this.Usuario = usuario;
            this.Tipo = 502; //Exportação de Relatorio em.pdf com sucesso
            this.Obs = "Filtros: " + filtros;
        }

        public void ExportarRelatorio_Erro(int usuario, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 501; //Exportação de Relatorio em .pdf com erro
            this.Obs = "ERRO: " + e.Message;
        }

        public void ConsultarRelatorio(int usuario, string filtros)
        {
            this.Usuario = usuario;
            this.Tipo = 504; // Consultar Relatório com sucesso
            this.Obs = "Filtros: " + filtros;
        }

        public void ConsultarRelatorio_Erro(int usuario, string filtros, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 503; //Consultar Relatório com Erro
            this.Obs = "ERRO: " + e.Message + ", Filtros: " + filtros;
        }

        public void Outro_Sucesso(int usuario, string descricao)
        {
            this.Usuario = usuario;
            this.Tipo = 900; //Outro (Sucesso)
            this.Obs = descricao;
        }

        public void Outro_Erro(int usuario, string descricao, Exception e)
        {
            this.Usuario = usuario;
            this.Tipo = 901; // Outro (Erro)
            this.Obs = "ERRO: " + e.Message + ", " + descricao;
        }

        public void CadastrarFuncionario(int usuario, long funcionario)
        {
            this.Usuario = usuario;
            this.Funcionario = funcionario;
            this.Tipo = 602; //Criação de Funcionario com Sucesso
        }

        public void CadastrarFuncionario_Erro(int usuario, long funcionario, Exception exp)
        {
            this.Usuario = usuario;
            this.Funcionario = funcionario;
            this.Obs = "ERRO: " + exp.Message;
            this.Tipo = 601; //Criação de Funcionario com Erro
        }

        public void AlterarFuncionario(int usuario, long funcionario)
        {
            this.Usuario = usuario;
            this.Funcionario = funcionario;
            this.Tipo = 603; //Alteração de Funcionario com Erro
        }

        public void AlterarFuncionario_Erro(int usuario, long funcionario, Exception exp)
        {
            this.Usuario = usuario;
            this.Funcionario = funcionario;
            this.Tipo = 604; //Alteração de Funcionario com Sucesso
            this.Obs = "ERRO: " + exp.Message;
        }

        public void EsqueciMinhaSenha_Envio(int usuario, string codigo)
        {
            this.Usuario = usuario;
            this.Tipo = 6; //Enviado link de redefinição de senha com sucesso
            this.Obs = "Código de Validação: " + codigo;
        }

        public void EsqueciMinhaSenha_Envio_Erro(int usuario, string codigo, Exception exp)
        {
            this.Usuario = usuario;
            this.Tipo = 5; //Enviado link de redefinição de senha com erro
            this.Obs = "ERRO: " + exp.Message + " / Código de Validação: " + codigo;
        }

        public void EsqueciMinhaSenha_Troca(int usuario)
        {
            this.Usuario = usuario;
            this.Tipo = 8; //Redefinição de senha por link com sucesso
        }

        public void EsqueciMinhaSenha_Troca_Erro(int usuario, Exception exp)
        {
            this.Usuario = usuario;
            this.Tipo = 7; //Redefinição de senha por link com erro
            this.Obs = "ERRO: " + exp.Message;
        }

        public void AlterarMinhaSenha(int usuario)
        {
            this.Usuario = usuario;
            this.Tipo = 10; //Alteração de senha pelo usuário com sucesso
        }

        public void AlterarMinhaSenha_Erro(int usuario, Exception exp)
        {
            this.Usuario = usuario;
            this.Tipo = 9; //Alteração de senha pelo usuário com erro
            this.Obs = "ERRO: " + exp.Message;
        }

    }
}
