using System.Globalization;
using System.Text;
using TransportationExceptionManagement.Application.Abstractions;
using TransportationExceptionManagement.Application.Cases.Models;

namespace TransportationExceptionManagement.Application.Exports;

public sealed class CaseCsvExportService(ICaseRepository repository) : ICaseCsvExportService
{
    private static readonly string[] Headers =
    [
        "CaseReference",
        "MovementReference",
        "OriginNode",
        "DestinationNode",
        "CarrierCode",
        "ExceptionType",
        "Severity",
        "Status",
        "Assignee",
        "CreatedAtUtc",
        "DueAtUtc",
        "ResolvedAtUtc"
    ];

    public async Task<string> ExportAsync(
        CaseFilter filter,
        CancellationToken cancellationToken)
    {
        var cases = await repository.ListForExportAsync(filter, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', Headers));

        foreach (var item in cases)
        {
            var fields = new[]
            {
                item.CaseReference,
                item.MovementReference,
                item.OriginNode,
                item.DestinationNode,
                item.CarrierCode,
                item.ExceptionType.ToString(),
                item.Severity.ToString(),
                item.Status.ToString(),
                item.Assignee ?? string.Empty,
                item.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                item.DueAtUtc.ToString("O", CultureInfo.InvariantCulture),
                item.ResolvedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty
            };

            builder.AppendLine(string.Join(',', fields.Select(Escape)));
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@')
        {
            value = $"'{value}";
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
