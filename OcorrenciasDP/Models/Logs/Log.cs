using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public void LogIn(int usuario)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 2; //Login efetuado com Sucesso 

        }

        public void LogIn_Erro(int usuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Obs = "ERRO: " + exp.Message;
            this.Tipo = 1; //Login efetuado com Erro 
        }
        public void LogOut(int usuario)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 4; //Logout efetuado com Sucesso
        }

        public void LogOut_ERRO(int usuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 3; //Logout efetuado com Sucesso
            this.Obs = "ERRO: " + exp.Message;
        }

        public void CadastrarUsuario(int usuario, int usuarioCadastrado)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioCadastrado;
            this.Tipo = 302; //Criação de Usuario com Sucesso
        }

        public void CadastrarUsuario_Erro(int usuario, string loginUsuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 301; //Criação de Usuario com Erro
            this.Obs = "ERRO: " + exp.Message + ", Login Usuário = " + loginUsuario;
        }
        
        public void ExcluirUsuario(int usuario, int usuarioExcluido)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioExcluido;
            this.Tipo = 304; //Exclusão de Usuario com Sucesso
        }

        public void ExcluirUsuario_Erro(int usuario, int usuarioExcluido, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioExcluido;
            this.Tipo = 303; //Exclusão de Usuario com Erro
            this.Obs = "ERRO: " + exp.Message;
        }

        public void AlterarUsuario(int usuario, int usuarioAlterado, bool alteraPerfil, string perfil)
        {
            this.Data = DateTime.Now;
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

        public void AlterarUsuario_Erro(int usuario, int usuarioAlterado, bool alteraPerfil, string perfil, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.UsuarioAlterado = usuarioAlterado;
            this.Obs = "ERRO: " + exp.Message;

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
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 202; //Envio de Ocorrencia com sucesso

        }
        
        public void IncluirOcorrencia_Erro(int usuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 201; //Envio de Ocorrencia com erro
            this.Obs = "ERRO: " + exp.Message;
        }

        public void ExcluirOcorrencia(int usuario, long ocorrencia)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 204; //Exclusão de Ocorrencia com sucesso
        }

        public void ExcluirOcorrencia_Erro(int usuario, long ocorrencia, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Ocorrencia = ocorrencia;
            this.Tipo = 203; //Exclusão de Ocorrencia com erro
            this.Obs = "ERRO: " + exp.Message;

        }

        public void LembreteEnviado(int usuario, int total)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 402; //Envio de Lembrete com Sucesso
            this.Obs = "Enviado " + total + " emails com sucesso";
        }

        public void LembreteEnviado_Erro(int usuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 401; //Envio de Lembrete com Erro
            this.Obs = "ERRO: " + exp.Message;
        }

        public void MensagemEnviada(int usuario, long mensagem)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 404; //Envio de Mensagem com Sucesso
            this.Mensagem = mensagem;
        }

        public void MensagemEnviada_Erro(int usuario, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 403; // Envio de Mensagem com Erro
            this.Obs = "ERRO: " + exp.Message;
        }

        public void Outro_Sucesso(int usuario, string log)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 900; //Outro (Sucesso)
            this.Obs = log;
        }

        public void Outro_Erro(int usuario, string log, Exception exp)
        {
            this.Data = DateTime.Now;
            this.Usuario = usuario;
            this.Tipo = 901; // Outro(Erro)
            this.Obs = "ERRO: " + exp.Message + ", " + log;
        }
    }
}
