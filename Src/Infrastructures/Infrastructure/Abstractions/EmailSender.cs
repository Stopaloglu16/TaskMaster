using Application.Common.Interfaces;
using Application.Common.Models;
using mailinator_csharp_client;
using mailinator_csharp_client.Models.Messages.Entities;
using mailinator_csharp_client.Models.Messages.Requests;
using mailinator_csharp_client.Models.Responses;

namespace Infrastructure.Abstractions;

public class EmailSender : IEmailSender
{

    private readonly string _emailApiTokenKey;
    private readonly MailinatorClient _mailinatorClient;
    private readonly string _mailinatorDomain;

    public EmailSender(string emailApiTokenKey, string mailinatorDomain)
    {
        _emailApiTokenKey = emailApiTokenKey;
        _mailinatorClient = new MailinatorClient(emailApiTokenKey);
        _mailinatorDomain = mailinatorDomain;
    }

    public async Task SendEmailAsync(EmailRequest request)
    {
        await Task.Delay(100);
        //Chaging to https://www.mailinator.com
        throw new NotImplementedException();
    }



    public async Task SendRegisterEmailAsync(string Username, string To, string Token)
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

            MessageBody = " <table style='width:50%;'><tbody><tr><td> <img  style='width:10%;' src='https://localhost:7056/img/carhire.jpeg'> </td></tr>" +
                            "<tr><td> <h2> <span class='welcometxt'>Welcome to Task Master! 📚</span></h2></td></tr>" +
                            "<tr><td>Click below to verify your account.</br> <a href='https://localhost:7056/register/" + Username + "/" + Token + "'>here</a></td></tr>" +
                            "<tr><td>Username: </br>" + Username + "</td></tr>" +
                            "</tbody></table>";

            string HtmlEnd = "</body></html>";


            MessageToPost messageToPost = new MessageToPost()
            {
                Subject = "Welcome to TaskMaster",
                From = "noreply@carhire.com",  //To email on live system
                //Text = HtmlBegin + MessageBody + HtmlEnd
                Text = $"{Username}|{Token}"
            };

            PostMessageRequest postMessageRequest = new PostMessageRequest() { Domain = _mailinatorDomain, Inbox = _mailinatorDomain, Message = messageToPost };
            PostMessageResponse postMessageResponse = await _mailinatorClient.MessagesClient.PostMessageAsync(postMessageRequest);
        }
        catch (Exception ex)
        {
            throw new Exception("SendRegisterEmailAsync " + ex.Message);
        }
    }


    public Task SendForgotPasswordEmailAsync(string Username, string To, string Link)
    {
        throw new NotImplementedException();
    }
}
