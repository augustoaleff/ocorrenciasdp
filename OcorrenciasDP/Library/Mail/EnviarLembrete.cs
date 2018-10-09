using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace OcorrenciasDP.Library.Mail
{
    public class EnviarLembrete
    {
        public static void EnviarMsgLembrete(int dias, List<String> emails)
        {

            string saudacao;

            if(DateTime.Now.Hour >= 3 && DateTime.Now.Hour <= 11) //Entre 3h e meio-dia ==> Bom Dia
            {
                saudacao = "Bom Dia <br />";
            }
            else if(DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 17) //Entre meio-dia e 18h ==> Boa Tarde
            {
                saudacao = "Boa Tarde <br />";
            }
            else //Entre 18h e 3h ==> Boa Noite
            {
                saudacao = "Boa Noite <br />";
            }

            string conteudo = string.Format("<br /><p>Não se esqueça de enviar as ocorrências, você está a mais de {0} dias sem enviar!</p>", dias);
            //string conteudo = string.Format("Nome: {0}<br /> E-mail: {1}<br /> Assunto: {2}<br /> Mensagem: {3}", contato.Nome, contato.Email, contato.Assunto, contato.Mensagem);

            SmtpClient smtp = new SmtpClient(Constants.ServidorSMTP, Constants.PortaSMTP)
            {
                EnableSsl = true,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(Constants.Usuario, Constants.Senha)
            };

            MailMessage mensagem = new MailMessage
            {
                From = new MailAddress("no-reply@eletroleste.com.br","Eletroleste"),
                Subject = "Lembrete",
                IsBodyHtml = true,
                Body = "<h2>Lembrete</h2>" + saudacao + conteudo + "<br /><br /><a href='http://www.eletroleste.com.br/OcorrenciasDP/'>Clique aqui para enviar</a>"

            };

            foreach (var email in emails)
            {
                if (email != null)
                {
                    mensagem.Bcc.Add(email);
                }

                
            }


            smtp.Send(mensagem);




            //mensagem.Subject = "Lembrete";

            //mensagem.IsBodyHtml = true;
            //mensagem.Body = "<h1>Lembrete</h1>" + saudacao + conteudo;






        }


    }
}
