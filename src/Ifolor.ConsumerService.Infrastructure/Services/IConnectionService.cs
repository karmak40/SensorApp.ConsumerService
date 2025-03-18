using RabbitMQ.Client;

namespace Ifolor.ConsumerService.Infrastructure.Services
{
    public interface IConnectionService {
        Task<IConnection> CreateConnectionWithRetryAsync(ConnectionFactory factory, CancellationToken cancellationToken);
    }

}
