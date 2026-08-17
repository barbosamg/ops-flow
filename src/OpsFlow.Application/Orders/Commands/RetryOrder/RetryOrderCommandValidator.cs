using FluentValidation;

namespace OpsFlow.Application.Orders.Commands.RetryOrder;

public sealed class RetryOrderCommandValidator :
    AbstractValidator<RetryOrderCommand>
{
    public RetryOrderCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(100);
    }
}
