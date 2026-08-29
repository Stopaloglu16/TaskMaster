using Application.Common.Interfaces;
using Application.Common.Models;
using mailinator_csharp_client;
using mailinator_csharp_client.Models.Messages.Entities;
using mailinator_csharp_client.Models.Messages.Requests;
using mailinator_csharp_client.Models.Responses;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Threading;

namespace Infrastructure.Abstractions;

public class EmailSender : IEmailSender
{

    private readonly string _emailApiTokenKey;
    private readonly MailinatorClient _mailinatorClient;
    private readonly string _mailinatorDomain;

    public EmailSender(string websiteUrl, string emailApiTokenKey, string mailinatorDomain)
    {
        _emailApiTokenKey = emailApiTokenKey;
        _mailinatorClient = new MailinatorClient(emailApiTokenKey);
        _mailinatorDomain = mailinatorDomain;
    }

    public async Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken)
    {
        try
        {

            var smtpClient = new SmtpClient("localhost", 25) // or 2525
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                EnableSsl = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("test@localhost.com"),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = false
            };

            foreach (var myMail in request.ToMail)
            {
                mailMessage.To.Add(myMail);
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            // Throws SocketException if cannot resolve
            // await System.Net.Dns.GetHostAddressesAsync(smtpHost);
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            throw new InvalidOperationException(
                $"SMTP host could not be resolved. " +
                "Set a valid SMTP_HOST environment variable or ensure the 'papercut' service is running and resolvable.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"SMTP host  could not be resolved. " +
                "Set", ex);
        }

        //await Task.Delay(100);
        ////Chaging to https://www.mailinator.com
        //throw new NotImplementedException();
    }


    public async Task SendRegisterEmailAsync(string Username, string To, string Token, CancellationToken cancellationToken)
    {

        try
        {
            //Test system to change logic!
            //Send emails to mailinator system
            string MessageBody = "";
            // TODO change url address
            string HtmlBegin = "<html><head><style> body { background: #eaeff1;  text-align: center; } " +
                               "table { border: 1px solid #1da5d1; border-radius: 13px;border-spacing: 0;background-color:white;} " +
                                ".welcometxt { font-size: x-large; color: #1da5d1 } " +
                                "</style></head><body>";

            MessageBody = " <table style='width:50%;'><tbody><tr><td> <img  style='width:50%;' src='https://localhost:7155/logos/TaskMasterLogo4.png'> </td></tr>" +
                            "<tr><td> <h2> <span class='welcometxt'>Welcome to Task Master! 📚</span></h2></td></tr>" +
                            "<tr><td>Click below to verify your account.</br> <a href='https://localhost:7155/register/" + Username + "/" + Token + "'>here</a></td></tr>" +
                            "<tr><td>Username: </br>" + Username + "</td></tr>" +
                            "</tbody></table>";

            string HtmlEnd = "</body></html>";


            //MessageToPost messageToPost = new MessageToPost()
            //{
            //    Subject = "Register",
            //    From = "noreply@taskmaster.com",  //To email on live system
            //    Text = HtmlBegin + MessageBody + HtmlEnd
            //    //Text = $"{Username}|{Token}"
            //};

            var smtpClient = new SmtpClient("localhost", 25) // or 2525
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                EnableSsl = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("test@localhost.com"),
                Subject = "Register",
                Body = HtmlBegin + MessageBody + HtmlEnd,
                IsBodyHtml = true
            };

            
            mailMessage.To.Add(To);
            

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            // Throws SocketException if cannot resolve
            // await System.Net.Dns.GetHostAddressesAsync(smtpHost);
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            throw new InvalidOperationException(
                $"SMTP host could not be resolved. " +
                "Set a valid SMTP_HOST environment variable or ensure the 'papercut' service is running and resolvable.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"SMTP host  could not be resolved. " +
                "Set", ex);
        }

    }

    //Send emails to mailinator system
    public async Task SendRegisterEmailMailinatorAsync(string Username, string To, string Token, CancellationToken cancellationToken)
    {
        try
        {
            //Test system to change logic!
            //Send emails to mailinator system
            string MessageBody = "";
            // TODO change url address
            string HtmlBegin = "<html><head><style> body { background: #eaeff1;  text-align: center; } " +
                               "table { border: 1px solid #1da5d1; border-radius: 13px;border-spacing: 0;background-color:white;} " +
                                ".welcometxt { font-size: x-large; color: #1da5d1 } " +
                                "</style></head><body>";

            MessageBody = " <table style='width:50%;'><tbody><tr><td> <img  style='width:10%;' src='https://localhost:7155/logos/TaskMasterLogo4.png'> </td></tr>" +
                            "<tr><td> <h2> <span class='welcometxt'>Welcome to Task Master! 📚</span></h2></td></tr>" +
                            "<tr><td>Click below to verify your account.</br> <a href='https://localhost:7155/register/" + Username + "/" + Token + "'>here</a></td></tr>" +
                            "<tr><td>Username: </br>" + Username + "</td></tr>" +
                            "</tbody></table>";

            string HtmlEnd = "</body></html>";


            MessageToPost messageToPost = new MessageToPost()
            {
                Subject = "Register",
                From = "noreply@taskmaster.com",  //To email on live system
                Text = HtmlBegin + MessageBody + HtmlEnd
                //Text = $"{Username}|{Token}"
            };

            PostMessageRequest postMessageRequest = new PostMessageRequest() { Domain = _mailinatorDomain, Inbox = _mailinatorDomain, Message = messageToPost };
            PostMessageResponse postMessageResponse = await _mailinatorClient.MessagesClient.PostMessageAsync(postMessageRequest);
        }
        catch (Exception ex)
        {
            throw new Exception("SendRegisterEmailAsync " + ex.Message);
        }
    }


    public async Task SendForgotPasswordEmailAsync(string Username, string To, string Token, CancellationToken cancellationToken)
    {
        try
        {
            //Test system to change logic!
            //Send emails to mailinator system
            string MessageBody = "";

            string HtmlBegin = "<html><head><style> body { background: #eaeff1;  text-align: center; } " +
                               "table { border: 1px solid #1da5d1; border-radius: 13px;border-spacing: 0;background-color:white;} " +
                                ".welcometxt { font-size: x-large; color: #1da5d1 } " +
                                "</style></head><body>";

            MessageBody = " <table style='width:50%;'><tbody><tr><td> <img  style='width:10%;' src='https://localhost:7081/img/carhire.jpeg'> </td></tr>" +
                            "<tr><td> <h2> <span class='welcometxt'>Welcome to Task Master! 📚</span></h2></td></tr>" +
                            "<tr><td>Click below to reset your password.</br> <a href='https://localhost:7155/forgotpassword?username=" + Username + "&token=" + Token + "'>here</a></td></tr>" +
                            "<tr><td>Username: </br>" + Username + "</td></tr>" +
                            "</tbody></table>";

            string HtmlEnd = "</body></html>";


            MessageToPost messageToPost = new MessageToPost()
            {
                Subject = "Forgot Password",
                From = "noreply@taskmaster.com",  //To email on live system
                Text = HtmlBegin + MessageBody + HtmlEnd
                //Text = $"{Username}|{Token}"
            };

            PostMessageRequest postMessageRequest = new PostMessageRequest() { Domain = _mailinatorDomain, Inbox = _mailinatorDomain, Message = messageToPost };
            PostMessageResponse postMessageResponse = await _mailinatorClient.MessagesClient.PostMessageAsync(postMessageRequest);
        }
        catch (Exception ex)
        {
            throw new Exception("SendForgotPasswordEmailAsync " + ex.Message);
        }
    }
}
