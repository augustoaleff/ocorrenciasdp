using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using OcorrenciasDP.Library.Globalization;

namespace OcorrenciasDP.Library.Mail
{
    public class EnviarLinkSenha
    {
        public static void EnviarLinkTrocarSenha(string email, string codigo)
        {
            string saudacao, conteudo;
            DateTime agora = Globalization.Globalization.HoraAtualBR();

            if (agora.Hour >= 3 && agora.Hour <= 11) //Entre 3h e meio-dia ==> Bom Dia
            {
                saudacao = "Bom Dia <br />";
            }
            else if (agora.Hour >= 12 && agora.Hour <= 17) //Entre meio-dia e 18h ==> Boa Tarde
            {
                saudacao = "Boa Tarde <br />";
            }
            else //Entre 18h e 3h ==> Boa Noite
            {
                saudacao = "Boa Noite <br />";
            }

            conteudo = "<br />  <a href='http://www.eletroleste.com.br/OcorrenciasDP/Home/TrocarSenha/key=" + codigo + "'>Click Aqui </a> para redefinir a sua senha: <br />";



            SmtpClient smtp = new SmtpClient(Constants.ServidorSMTP, Constants.PortaSMTP)
            {
                EnableSsl = true,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(Constants.Usuario, Constants.Senha)
            };

            MailMessage mensagem = new MailMessage
            {
                From = new MailAddress("no-reply@eletroleste.com.br", "Eletroleste"),
                Subject = "Redefinição de Senha",
                IsBodyHtml = true,
                Body = saudacao +
                conteudo +
                "<br /><br /><font size='1'>Mensagem Automática, favor não responder. Enviada: " + Globalization.Globalization.DataAtualExtensoBR() + "</font>"

            };

            mensagem.Bcc.Add(email);
            smtp.Send(mensagem);

        }

    }
}
