using mailinator_csharp_client;
using mailinator_csharp_client.Models.Messages.Entities;
using mailinator_csharp_client.Models.Messages.Requests;
using mailinator_csharp_client.Models.Responses;
using Microsoft.Extensions.Configuration;

namespace WebApiAuth.FunctionalTests
{
    public abstract class EmailTestBase
    {
        protected string MailinatorApiToken { get; private set; } = string.Empty;
        protected string MailinatorDomain { get; private set; } = string.Empty;

        protected readonly MailinatorClient mailinatorClient;

        protected EmailTestBase()
        {
            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<TestBase>()
           .Build();

            MailinatorApiToken = configuration["MailinatorApiToken"];
            MailinatorDomain = configuration["MailinatorDomain"];

            mailinatorClient = new MailinatorClient(MailinatorApiToken);
        }

        public Task<PostMessageResponse> PostNewMessageAsync(string domain, string inbox, MessageToPost messageToPost)
        {
            var request = new PostMessageRequest() { Domain = domain, Inbox = inbox, Message = messageToPost };
            return mailinatorClient.MessagesClient.PostMessageAsync(request);
        }
    }

    public abstract class TestBase
    {
    }

}
