using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Outbox
{
    internal sealed class TestInstanceConfiguration : EntityTypeConfiguration<TestInstance>
    {
        public TestInstanceConfiguration()
        {
            ToTable("TEST");
            HasKey(e => e.Id);

            Property(e => e.Id)
               .HasColumnName("ID")
               .HasColumnType("NUMBER")
               .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
               .IsRequired();
        }
    }
}
