using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;

namespace Outbox
{
    internal class OutboxDbContext : DbContext
    {
        public OutboxDbContext(DbConnection connection) : base(connection, false)
        {
            Database.SetInitializer<OutboxDbContext>(null);
            SetupDebugLogging();
        }

        [Conditional("DEBUG")]
        private void SetupDebugLogging()
        {
            Database.Log = s =>
            {
                Debug.WriteLine(s);
            };

        }
        public DbSet<TestInstance> TestInstances => Set<TestInstance>();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TESTUSER");
            modelBuilder.Configurations.Add(new TestInstanceConfiguration());
        }
    }
}
