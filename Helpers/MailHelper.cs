using Microsoft.Extensions.Configuration;
using MimeKit;
using System;

namespace GestaoEscolarWeb.Helpers
{
    public class MailHelper : IMailHelper   
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration) // injetar IConfiguration para buscar os dados do email
        {
            _configuration = configuration;
        }
        public Response SendEmail(string to, string subject, string body)
        {
            //buscar dados
            var nameFrom = _configuration["Mail:NameFrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            var message = new MimeMessage(); //classe do mailkit
            message.From.Add(new MailboxAddress(nameFrom, from)); //quem vai mandar
            message.To.Add(new MailboxAddress(to, to)); //para quem vou enviar
            message.Subject = subject; //assunto

            var bodybuilder = new BodyBuilder();
            {
                bodybuilder.HtmlBody = body; //cria o body do email
            }
            message.Body = bodybuilder.ToMessageBody(); //une o body ao email


            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())// usa o protocolo Smtp para envio de emails
                {
                    client.Connect(smtp, int.Parse(port), false); //conecta ao email
                    client.Authenticate(from, password); //autentica
                    client.Send(message); //envia
                    client.Disconnect(true); //fecha conexão
                }

            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };

            }

            //se executar o try, correu bem
            return new Response
            {
                IsSuccess = true
            };

        }
    }
}

