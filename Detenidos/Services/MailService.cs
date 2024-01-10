using Detenidos.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Detenidos.Utilidades;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Detenidos.Services
{
    public class MailService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public MailService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public void SendEmailOutlook(string asunto, string mensaje,string destinatario,List<string> conCopia)
        {           
            try
            {
                ConfigCorreo datosCorreo =  _context.ConfigCorreo.Where(x => !x.Borrado && x.Vigente).FirstOrDefault();              

                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress("Sistema REFIC",datosCorreo.Usuario));
                    mailMessage.To.Add(new MailboxAddress("",destinatario));

                if (conCopia != null)
                {
                    foreach (var item in conCopia)
                    {
                        mailMessage.Cc.Add(new MailboxAddress("", item));
                    }
                }
                mailMessage.Subject = asunto;
                    var builder = new BodyBuilder();
                    builder.HtmlBody = mensaje;
                    //builder.Attachments.Add("path");
                    mailMessage.Body = builder.ToMessageBody();

                    using var smtpClient = new SmtpClient();
                    smtpClient.Connect(datosCorreo.Host, datosCorreo.Port, SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(datosCorreo.Usuario, Security.Decrypt(datosCorreo.Password));
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
            }
            catch (Exception e)
            {

            }            
        }

    }
}
