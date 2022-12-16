using NServiceBus;

namespace Outbox
{
    internal class OutboxCommand : ICommand
    {
        public long Id { get; set; }
    }
}