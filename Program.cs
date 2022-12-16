using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.TransactionalSession;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Outbox
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string c_instanceName = "Test";
            const string c_sqlServerConnectionString = "Server=localhost;Database=NServiceBus;Trusted_Connection=True;MultipleActiveResultSets=true";
            string oracleConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString.Oracle (ODP.NET)"].ConnectionString;

            var config = new EndpointConfiguration(c_instanceName);

            var outbox = config.EnableOutbox();
            //outbox.UsePessimisticConcurrencyControl();
            config.EnableInstallers();

            var persistence = config.UsePersistence<SqlPersistence>();
            persistence.ConnectionBuilder(() => new OracleConnection(oracleConnectionString));
            persistence.EnableTransactionalSession();
            var dialectSettings = persistence.SqlDialect<SqlDialect.Oracle>();
            SqlPersistenceConfig.Schema(dialectSettings, "TESTUSER");

            var transport = config.UseTransport<SqlServerTransport>();
            transport.ConnectionString(c_sqlServerConnectionString);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            config.UniquelyIdentifyRunningInstance()
                  .UsingNames
                  (
                      instanceName: c_instanceName,
                      hostName: Environment.MachineName
                  );
            transport.NativeDelayedDelivery();

            var services = new ServiceCollection();
            var startableEndpoint = EndpointWithExternallyManagedContainer.Create(config, services);
            var builder = services.BuildServiceProvider();
            var endpoint = await startableEndpoint.Start(builder);

            for (; ; )
            {
                Console.WriteLine("How many messages would you like to send?");
                if (!int.TryParse(Console.ReadLine(), out int amount))
                {
                    Console.WriteLine("Not a number");
                    continue;
                }

                var scope = builder.CreateScope();
                using (var session = scope.ServiceProvider.GetRequiredService<ITransactionalSession>())
                {
                    await session.Open(new SqlPersistenceOpenSessionOptions());
                    var storageSession = session.SynchronizedStorageSession.SqlPersistenceSession();

                    using (var context = new OutboxDbContext(storageSession.Connection))
                    {
                        context.Database.UseTransaction(storageSession.Transaction);

                        for (int i = 0; i < amount; i++)
                        {
                            context.TestInstances.Add(new TestInstance { Id = i + 1 });
                            await session.Send("Test", new OutboxCommand { Id = i + 1 });
                        }

                        await context.SaveChangesAsync();
                    }

                    await session.Commit();

                    Console.WriteLine("Messages sent");
                }
            }
        }
    }
}

