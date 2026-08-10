using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransportationExceptionManagement.Domain.Entities;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Infrastructure.Persistence.Configurations;

public sealed class TransportationExceptionCaseConfiguration
    : IEntityTypeConfiguration<TransportationExceptionCase>
{
    public void Configure(EntityTypeBuilder<TransportationExceptionCase> builder)
    {
        var timestampConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.ToUnixTimeMilliseconds(),
            value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        var nullableTimestampConverter = new ValueConverter<DateTimeOffset?, long?>(
            value => value.HasValue ? value.Value.ToUnixTimeMilliseconds() : null,
            value => value.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value) : null);

        builder.ToTable("TransportationExceptionCases");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.CaseReference)
            .HasMaxLength(CaseFieldLimits.CaseReferenceMaxLength)
            .UseCollation("NOCASE")
            .IsRequired();
        builder.HasIndex(entity => entity.CaseReference).IsUnique();

        builder.Property(entity => entity.MovementReference)
            .HasMaxLength(CaseFieldLimits.MovementReferenceMaxLength)
            .IsRequired();
        builder.Property(entity => entity.OriginNode)
            .HasMaxLength(CaseFieldLimits.NodeMaxLength)
            .IsRequired();
        builder.Property(entity => entity.DestinationNode)
            .HasMaxLength(CaseFieldLimits.NodeMaxLength)
            .IsRequired();
        builder.Property(entity => entity.CarrierCode)
            .HasMaxLength(CaseFieldLimits.CarrierCodeMaxLength)
            .IsRequired();
        builder.Property(entity => entity.Description)
            .HasMaxLength(CaseFieldLimits.DescriptionMaxLength)
            .IsRequired();
        builder.Property(entity => entity.Assignee)
            .HasMaxLength(CaseFieldLimits.AssigneeMaxLength);
        builder.Property(entity => entity.ResolutionSummary)
            .HasMaxLength(CaseFieldLimits.ResolutionSummaryMaxLength);

        builder.Property(entity => entity.ExceptionType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(entity => entity.Severity)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(entity => entity.Status)
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();

        builder.Property(entity => entity.CreatedAtUtc)
            .HasConversion(timestampConverter)
            .HasColumnType("INTEGER");
        builder.Property(entity => entity.UpdatedAtUtc)
            .HasConversion(timestampConverter)
            .HasColumnType("INTEGER");
        builder.Property(entity => entity.DueAtUtc)
            .HasConversion(timestampConverter)
            .HasColumnType("INTEGER");
        builder.Property(entity => entity.ResolvedAtUtc)
            .HasConversion(nullableTimestampConverter)
            .HasColumnType("INTEGER");

        builder.HasIndex(entity => entity.Status);
        builder.HasIndex(entity => entity.Severity);
        builder.HasIndex(entity => entity.ExceptionType);
        builder.HasIndex(entity => entity.Assignee);
        builder.HasIndex(entity => entity.CreatedAtUtc);
        builder.HasIndex(entity => entity.DueAtUtc);

        builder.HasMany(entity => entity.Notes)
            .WithOne()
            .HasForeignKey(note => note.TransportationExceptionCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(entity => entity.Notes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
