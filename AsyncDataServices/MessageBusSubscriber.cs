using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ILogger = Serilog.ILogger;

namespace CommandsService.AsyncDataServices
{
  public class MessageBusSubscriber : BackgroundService
  {
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;

    public MessageBusSubscriber(ILogger logger, IConfiguration configuration, IEventProcessor eventProcessor)
    {
      _logger = logger;
      _configuration = configuration;
      _eventProcessor = eventProcessor;

      _logger.Information("--> Initializing MessageBusSubscriber.");
      InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
      var factory = new ConnectionFactory()
      {
        HostName = _configuration["RabbitMQHost"],
        Port = int.Parse(_configuration["RabbitMQPort"])
      };

      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
      _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
      _queueName = _channel.QueueDeclare().QueueName;
      _channel.QueueBind(queue: _queueName,
                          exchange: "trigger",
                          routingKey: "");

      _logger.Information("--> Listening on the Message Bus...");

      _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      stoppingToken.ThrowIfCancellationRequested();

      var consumer = new EventingBasicConsumer(_channel);

      consumer.Received += (ModuleHandle, ea) =>
      {
        _logger.Information("--> Event Received");

        var body = ea.Body;
        var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

        _eventProcessor.ProcessEvent(notificationMessage);
      };

      _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

      return Task.CompletedTask;
    }

    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
      _logger.Information("--> Connection Shutdown.");
    }

    public override void Dispose()
    {
      if(_channel.IsOpen)
      {
        _channel.Close();
        _connection.Close();
      }

      base.Dispose();
    }
  }
}