using FluentValidation;
using Library.Contracts.v1.Books.Request;

namespace Library.Controllers.Validations.v1;

public sealed class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название книги обязательно.")
            .MaximumLength(500).WithMessage("Название книги не может превышать 500 символов.");

        RuleFor(x => x.Authors)
            .NotEmpty().WithMessage("Автор или авторы книги обязательны.")
            .Must(authors => authors?.All(a => !string.IsNullOrWhiteSpace(a)) == true)
            .WithMessage("Все авторы должны быть непустыми строками.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1800, DateTime.Now.Year).WithMessage("Год издания должен быть между 1800 и настоящем годом.");
    }
}