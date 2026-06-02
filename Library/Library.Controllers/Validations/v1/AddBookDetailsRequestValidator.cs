using FluentValidation;
using Library.Contracts.v1.Books.Request;

namespace Library.Controllers.Validations.v1;

public sealed class AddBookDetailsRequestValidator : AbstractValidator<AddBookDetailsRequest>
{
    public AddBookDetailsRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}