using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class FileJobUploadConfiguration : IEntityTypeConfiguration<FileJobUpload>
{
    public void Configure(EntityTypeBuilder<FileJobUpload> builder)
    {
        // Idempotency for the chunked upload: the page posts one request per 10 CSV rows, so a
        // retried or duplicated chunk would otherwise insert its rows twice.
        builder.HasIndex(upload => new { upload.FileJobId, upload.BatchKey }).IsUnique();
    }
}
