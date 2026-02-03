using System.Net;
using System.Net.Mail;

namespace EspSupportUtils;

public class SendGmail(string userName, string password)
{


    public void Send(string to, string subject, string body)
    {
        try
        {
            // Gmail SMTP Server details
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;


            // Setup MailMessage
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(userName);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true; // Set to true if using HTML

            // Configure SMTP Client
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(userName, password);
            smtpClient.EnableSsl = true; // Enable SSL (Required for Gmail)

            // Send Email
            smtpClient.Send(mail);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
