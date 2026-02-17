using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StargateAPI.Business.Data
{
    [Table("ApiLog")]
    public class ApiLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Level { get; set; } = string.Empty;

        public string RequestType { get; set; } = string.Empty;

        public string RequestData { get; set; } = string.Empty;

        public string? ResponseData { get; set; }

        public string? ExceptionMessage { get; set; }

        public string? ExceptionStackTrace { get; set; }

        public int? StatusCode { get; set; }

        public long DurationMs { get; set; }
    }

    public class ApiLogConfiguration : IEntityTypeConfiguration<ApiLog>
    {
        public void Configure(EntityTypeBuilder<ApiLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Timestamp).IsRequired();
            builder.Property(x => x.Level).IsRequired().HasMaxLength(50);
            builder.Property(x => x.RequestType).IsRequired().HasMaxLength(200);
            builder.Property(x => x.RequestData).IsRequired();
            builder.Property(x => x.ResponseData);
            builder.Property(x => x.ExceptionMessage);
            builder.Property(x => x.ExceptionStackTrace);
            builder.Property(x => x.StatusCode);
            builder.Property(x => x.DurationMs).IsRequired();
        }
    }
}
