using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Outbox
{
    internal sealed class OutboxCommandHandler : IHandleMessages<OutboxCommand>
    {
        public Task Handle(OutboxCommand message, IMessageHandlerContext context)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Message with id {message.Id} was handled");
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }
    }
}