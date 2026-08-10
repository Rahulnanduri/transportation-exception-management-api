using System.Collections.ObjectModel;
using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Domain.Policies;

/// <summary>
/// Provides synthetic demonstration thresholds created solely for this portfolio project.
/// These values are not operational standards from any employer or transportation provider.
/// </summary>
public static class IllustrativeSlaPolicy
{
    public static IReadOnlyDictionary<ExceptionSeverity, TimeSpan> Thresholds { get; } =
        new ReadOnlyDictionary<ExceptionSeverity, TimeSpan>(
            new Dictionary<ExceptionSeverity, TimeSpan>
            {
                [ExceptionSeverity.Critical] = TimeSpan.FromHours(2),
                [ExceptionSeverity.High] = TimeSpan.FromHours(4),
                [ExceptionSeverity.Medium] = TimeSpan.FromHours(8),
                [ExceptionSeverity.Low] = TimeSpan.FromHours(24),
            });

    public static TimeSpan GetTarget(ExceptionSeverity severity)
    {
        if (!Thresholds.TryGetValue(severity, out TimeSpan target))
        {
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown severity.");
        }

        return target;
    }

    public static DateTimeOffset CalculateDueAt(
        ExceptionSeverity severity,
        DateTimeOffset createdAtUtc) => CalculateDueAtUtc(createdAtUtc, severity);

    public static DateTimeOffset CalculateDueAtUtc(
        DateTimeOffset createdAtUtc,
        ExceptionSeverity severity) => createdAtUtc.ToUniversalTime().Add(GetTarget(severity));
}
