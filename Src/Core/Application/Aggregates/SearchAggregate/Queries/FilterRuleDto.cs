namespace Application.Aggregates.SearchAggregate.Queries
{
    public record FilterRuleDto
    {
        public int FieldId { get; init; }
        public int OperatorId { get; init; }
        public List<string> Values { get; init; } = new();
        public string Logic { get; init; } = "AND"; // AND / OR
    }
}
