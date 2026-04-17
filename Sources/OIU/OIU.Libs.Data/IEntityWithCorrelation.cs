using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OIU.Libs.Data;


/// <summary>
/// Indicates that the entity has a correlation identifier used to track the async process.
/// </summary>
public interface IEntityWithCorrelation
{
    /// <summary>
    /// Correlation identifier used to track the async process
    /// </summary>
    string? CorrelationId { get; set; }
}
/// <summary>
/// 
/// </summary>
public static class IEntityWithCorrelationExtensions
{
    extension<T>(EntityTypeBuilder<T> @this) where T : class, IEntityWithCorrelation
    {
        /// <summary>
        /// Configue the entity type builder with the properties of the job. This method should be called in the entity type builder of the entities that implement the IEntityWithJob interface.
        /// </summary>
        public void ConfigureCorrelationEntityTypeBuilder()
        {
            const int CorrelationIdMaxLength = 36;

            @this.Property(p => p.CorrelationId).HasColumnOrder(1_000).HasMaxLength(CorrelationIdMaxLength).HasComment("Correlation identifier used to track the full verification process");
        }
    }
}