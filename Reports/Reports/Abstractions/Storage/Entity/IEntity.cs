using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reports.Abstractions.Storage.Entity;

public interface IEntity
{
    /// <summary> Идентификатор сущности </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    Guid Id { get; set; }

    /// <summary> Дата создания </summary>
    DateTime CreatedAt { get; set; }

    /// <summary> Дата обновления </summary>
    DateTime? UpdatedAt { get; set; }
}