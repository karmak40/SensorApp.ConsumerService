using RabbitMQ.Client;

namespace Ifolor.ConsumerService.Infrastructure.Services
{
    public interface IConnectionService {
        Task<IConnection> CreateConnectionWithRetryAsync(IConnectionFactory factory, CancellationToken cancellationToken);
    }
}
