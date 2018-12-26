using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using OcorrenciasDP.Library.Globalization;

namespace OcorrenciasDP.Library.Mail
{
    public class EnviarLembrete
    {
        public static void EnviarMsgLembrete(int dias, List<String> emails)
        {
            
            string saudacao, conteudo;
            DateTime agora = Globalization.Globalization.HoraAtualBR();

            if(agora.Hour >= 3 && agora.Hour <= 11) //Entre 3h e meio-dia ==> Bom Dia
            {
                saudacao = "Bom Dia <br />";
            }
            else if(agora.Hour >= 12 && agora.Hour <= 17) //Entre meio-dia e 18h ==> Boa Tarde
            {
                saudacao = "Boa Tarde <br />";
            }
            else //Entre 18h e 3h ==> Boa Noite
            {
                saudacao = "Boa Noite <br />";
            }
           
            if(dias == 1)
            {
                conteudo = "<br /><p>Não se esqueça de enviar as ocorrências, você está a mais de 1 dia sem enviar!</p>";
            }
            else
            {
                conteudo = string.Format("<br /><p>Não se esqueça de enviar as ocorrências, você está a mais de {0} dias sem enviar!</p>", dias);
            }
             
            //string conteudo = string.Format("Nome: {0}<br /> E-mail: {1}<br /> Assunto: {2}<br /> Mensagem: {3}", contato.Nome, contato.Email, contato.Assunto, contato.Mensagem);

            SmtpClient smtp = new SmtpClient(Constants.ServidorSMTP, Constants.PortaSMTP)
            {
                EnableSsl = true,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(Constants.Usuario, Constants.Senha)
            };

            MailMessage mensagem = new MailMessage
            {
                From = new MailAddress("no-reply@eletroleste.com.br", "Eletroleste"),
                Subject = "Lembrete",
                IsBodyHtml = true,

                Body = saudacao +
                conteudo + "<br />" +
                "<a href='http://www.eletroleste.com.br/Agenda/'><h2>Clique aqui para enviar<h2></a>" +
                "<br /><br /><font size='1'>Mensagem Automática, favor não responder. Enviada: " + Globalization.Globalization.DataAtualExtensoBR() + "</font>"

            };

            foreach (var email in emails)
            {
                if (email != null)
                {
                    mensagem.Bcc.Add(email);
                }
            }
            
            smtp.Send(mensagem);
            
        }

       

    }
}
