using FluentValidation;

namespace OpsFlow.Application.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderCommandValidator :
    AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.ProviderId).NotEmpty();
        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(999_999_999_999.99m);
        RuleFor(command => command.Notes).MaximumLength(1_000);
        RuleFor(command => command.RowVersion)
            .NotEmpty()
            .Must(BeBase64)
            .WithMessage("RowVersion must be a valid Base64 value.");
    }

    private static bool BeBase64(string value)
    {
        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out _);
    }
}
