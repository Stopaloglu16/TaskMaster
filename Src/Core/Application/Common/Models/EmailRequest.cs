namespace Application.Common.Models;

public class EmailRequest
{
    public EmailRequest(string fromMail, string fromDisplayName, List<string> toMail, string subject, string body, bool isHtml)
    {
        FromMail = fromMail;
        FromDisplayName = fromDisplayName;
        ToMail = toMail;
        Subject = subject;
        Body = body;
        IsHtml = isHtml;
    }

    public string FromMail { get; set; }
    public string FromDisplayName { get; set; }
    public List<string> ToMail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
}
