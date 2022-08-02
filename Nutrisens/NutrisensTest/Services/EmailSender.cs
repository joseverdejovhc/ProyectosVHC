using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Nutrisens.Data;

namespace Nutrisens.Services
{
    public class EmailSender
    {
        private readonly string host = "velvira.vegenat.net";
        private readonly string remitente = "notificaciones@vegenathc.es";
        private readonly int port = 25;
        private readonly bool enablessl = false;
        private readonly string pass = "vN1ALR12015";
        private AccesoDatos accesoDatos;
        public EmailSender()
        {
            accesoDatos = new AccesoDatos();
        }

        public void Enviar(string destinatario, string subject, string htmlMessage)
        {

            try
            {
                // Quitar esto en Producción, está puesto para que me lleguen todos los correos a mí. Si no, al registrar un usuario le llegaría un mail
                // destinatario = "mleal@vegenathc.es; docobos@vegenathc.es; jverdejo@vegenathc.es";
                // destinatario = "joseverdejo100@gmail.com;jverdejo@vegenathc.es";

                MailMessage message = new MailMessage();
                message.From = new MailAddress(remitente);
                message.Subject = subject;

                if (destinatario.Contains(";"))
                {
                    foreach (var address in destinatario.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        message.To.Add(new MailAddress(address));
                    }
                }
                else
                {
                    message.To.Add(new MailAddress(destinatario));
                }
                
                message.Body = "<html><body> " + htmlMessage + " </body></html>";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(remitente, pass),
                    EnableSsl = enablessl,
                };

                smtpClient.Send(message);
            }
            catch(Exception ex)
            {
                accesoDatos.saveLogError(ex.Message);
            }
           
        }
    }

}

