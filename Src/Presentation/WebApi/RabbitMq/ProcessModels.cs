namespace WebApi.RabbitMq
{
    public class ProcessModels
    {
    }

    // Contracts/ProcessItem.cs
    public record ProcessItem(string Id, string Name);

    // Contracts/ProcessResult.cs
    public record ProcessResult(string Id, string Status, string Output);

    // Contracts/ProcessMessage.cs
    public record ProcessMessage(string RequestId, ProcessItem Item);
}
