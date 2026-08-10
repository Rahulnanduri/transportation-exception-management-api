using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransportationExceptionManagement.Domain.Entities;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Infrastructure.Persistence.Configurations;

public sealed class CaseNoteConfiguration : IEntityTypeConfiguration<CaseNote>
{
    public void Configure(EntityTypeBuilder<CaseNote> builder)
    {
        var timestampConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.ToUnixTimeMilliseconds(),
            value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.ToTable("CaseNotes");
        builder.HasKey(note => note.Id);
        builder.Property(note => note.Author)
            .HasMaxLength(CaseFieldLimits.NoteAuthorMaxLength)
            .IsRequired();
        builder.Property(note => note.Text)
            .HasMaxLength(CaseFieldLimits.NoteTextMaxLength)
            .IsRequired();
        builder.Property(note => note.CreatedAtUtc)
            .HasConversion(timestampConverter)
            .HasColumnType("INTEGER");
        builder.HasIndex(note => new { note.TransportationExceptionCaseId, note.CreatedAtUtc });
    }
}
