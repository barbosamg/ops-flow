using System.ComponentModel.DataAnnotations;

namespace OpsFlow.Web.Models.Orders;

public sealed class OrderFormModel
{
    [Required(ErrorMessage = "Selecione um cliente.")]
    public Guid? CustomerId { get; set; }

    [Required(ErrorMessage = "Selecione um provedor.")]
    public Guid? ProviderId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "999999999999.99",
        ErrorMessage = "Informe um valor maior que zero.",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [StringLength(1000, ErrorMessage = "Use no máximo 1000 caracteres.")]
    public string? Notes { get; set; }

    public string? RowVersion { get; set; }

    public static OrderFormModel FromDetails(OrderDetails order) =>
        new()
        {
            CustomerId = order.CustomerId,
            ProviderId = order.ProviderId,
            Amount = order.Amount,
            Notes = order.Notes,
            RowVersion = order.RowVersion
        };

    public OrderUpsertRequest ToRequest() =>
        new(
            CustomerId!.Value,
            ProviderId!.Value,
            Amount,
            string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            RowVersion);
}
