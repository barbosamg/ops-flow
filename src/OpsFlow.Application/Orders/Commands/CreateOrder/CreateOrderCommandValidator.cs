using FluentValidation;

namespace OpsFlow.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator :
    AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.ProviderId).NotEmpty();
        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(999_999_999_999.99m);
        RuleFor(command => command.Notes).MaximumLength(1_000);
    }
}
