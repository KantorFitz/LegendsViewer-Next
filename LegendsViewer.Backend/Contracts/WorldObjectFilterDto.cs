namespace LegendsViewer.Backend.Contracts;

public class WorldObjectFilterDto
{
    public List<FilterRuleDto> Filters { get; set; } = new();
    public string? SearchTerm { get; set; }
}

public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    GreaterThan,
    LessThan,
}

public class FilterRuleDto
{
    public string PropertyName { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}