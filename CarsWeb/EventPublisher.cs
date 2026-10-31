using RabbitMQ.Client;
using RabbitMQ.Client.Impl;
using System.Text.Json.Serialization;

namespace CarsWeb
{
    public class Message{
        [JsonPropertyName("eventType")]
        public string? Event { set; get; }

        [JsonPropertyName("eventData")]
        public object? Payload { set; get; }
    }

    public class EventPublisher : IDisposable
    {
        private readonly ConnectionFactory connectionFactory;

        private IConnection conn;
        private IChannel channel;

        private string exchange = "";
        private string queue = "";
        public EventPublisher(string host)
        {
            connectionFactory = new ConnectionFactory { HostName = host };
        }

        public void Dispose()
        {
            conn.Dispose();
            channel.Dispose();
        }
        public async Task InitAsync(string exchange, string queue)
        {
            conn = await connectionFactory.CreateConnectionAsync();
            channel = await conn.CreateChannelAsync();

            this.exchange = exchange;
            this.queue = queue;

            await channel.ExchangeDeclareAsync(exchange,ExchangeType.Fanout);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queue, exchange, "");

        }

        public async Task PublishAsync(byte[] body)
        {
            if (channel == null)
            {
                throw new InvalidOperationException("Не инициализирован канал передачи сообщений");
            }

            await channel.BasicPublishAsync(exchange,"", body);
        }
    }
}
