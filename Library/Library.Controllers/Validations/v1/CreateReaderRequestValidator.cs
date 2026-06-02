using FluentValidation;
using Library.Contracts.v1.Reader.Request;

namespace Library.Controllers.Validations.v1;

public class CreateReaderRequestValidator : AbstractValidator<CreateReaderRequest>
{
    public CreateReaderRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("ФИО должно быть заполнено")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("ФИО должна быть непустая строка.")
            .MinimumLength(5).WithMessage("Минимальная длина ФИО: 5 символов.")
            .MaximumLength(200).WithMessage("Максимальная длина ФИО: 200 символов");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона должен быть заполнен.")
            .Must(x => x.Length > 11 && x.StartsWith("+7")).WithMessage("Неверный формат номера телефона.");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty().WithMessage("Срок истечений должен быть указан.")
            .GreaterThan(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Срок истечения должен быть позднее сегодняшнего дня.");
    }
}