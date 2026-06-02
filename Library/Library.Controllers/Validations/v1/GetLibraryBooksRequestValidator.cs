using FluentValidation;
using Library.Contracts.v1.Library.Requests;

namespace Library.Controllers.Validations.v1;

public class GetLibraryBooksRequestValidator : AbstractValidator<GetLibraryBooksRequest>
{
    public GetLibraryBooksRequestValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Категория должна быть от 1 до 3.");
        
        RuleFor(x => x.Page)
            .NotEmpty().WithMessage("Надо указать номер страницы.")
            .GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть минимум 1.");

        RuleFor(x => x.PageSize)
            .NotEmpty().WithMessage("Надо указать размер страницы.")
            .InclusiveBetween(1, 100).WithMessage("Размер страницы должен быть в диапазоне от 1 до 100.");
    }
}