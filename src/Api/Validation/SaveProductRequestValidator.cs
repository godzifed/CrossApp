using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Артикул обов'язковий.")
            .MaximumLength(50).WithMessage("Артикул не може перевищувати 50 символів.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова.")
            .MaximumLength(200).WithMessage("Назва не може перевищувати 200 символів.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Одиниця виміру обов'язкова.")
            .MaximumLength(20).WithMessage("Одиниця виміру не може перевищувати 20 символів.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Кількість не може бути від'ємною.");
    }
}