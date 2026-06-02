namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Учебное пособие
/// </summary>
public sealed class EducationalBookEntity : AbstractBookEntity
{
    /// <summary>Учебная область</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Учебный уровень</summary>
    public string GradeLevel { get; set; } = string.Empty;
}