using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Messaging;
using Ifolor.ConsumerService.Infrastructure.Persistance;
using Ifolor.ConsumerService.Infrastructure.Services;
using IfolorConsumerService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SQLiteConfig>(context.Configuration.GetSection("SQLite"));
                    services.Configure<RabbitMQConfig>(context.Configuration.GetSection("RabbitMQ"));

                    // SQLite
                    services.AddDbContextFactory<ConsumerDbContext>((serviceProvider, options) =>
                    {
                        var sqliteConfig = serviceProvider.GetRequiredService<IOptions<SQLiteConfig>>();
                        var connectionString = sqliteConfig.Value.ConnectionString;

                        if (string.IsNullOrEmpty(connectionString))
                        {
                            throw new InvalidOperationException("SQLite connection string is null or empty.");
                        }
                        options.UseSqlite(connectionString);
                    });

                    services.AddScoped<IEventRepository, EventRepository>();
                    services.AddScoped<IEventProcessor, EventProcessor>();
                    services.AddScoped<ISensorService, SensorService>();
                    services.AddScoped<IMessageConsumer, RabbitMQConsumer>();
                    services.AddScoped<IConnectionService, ConnectionService>();

                    services.AddHostedService<ControlService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsumerDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();












//var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
//using var connection = await factory.CreateConnectionAsync();
//using var channel = await connection.CreateChannelAsync();

//await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
//    arguments: null);

//Console.WriteLine(" [*] Waiting for messages.");

//var consumer = new AsyncEventingBasicConsumer(channel);


//// Настройка контекста базы данных
//var options = new DbContextOptionsBuilder<EventDbContext>()
//    .UseSqlite("Data Source=events.db")
//    .Options;


//consumer.ReceivedAsync += (model, ea) =>
//{
//    var body = ea.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);
//    Console.WriteLine($" [x] Received {message}");
//    return Task.CompletedTask;
//};

//await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

//Console.WriteLine(" Press [enter] to exit.");
//Console.ReadLine();