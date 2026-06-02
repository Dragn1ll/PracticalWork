namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Научная литература
/// </summary>
public sealed class ScientificBookEntity : AbstractBookEntity
{
    /// <summary>Область исследований</summary>
    public string ResearchField { get; set; } = string.Empty;

    /// <summary>Издатель</summary>
    public string Publisher { get; set; } = string.Empty;
}