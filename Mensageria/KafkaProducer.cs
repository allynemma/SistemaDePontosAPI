using Confluent.Kafka;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Mensageria
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducer(string bootstrapServers, string topic)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topic = topic;
            Console.WriteLine($"Kafka Producer inicializado com bootstrapsServer: {bootstrapServers} e topic: {_topic}");
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                Console.WriteLine($"Mensagem entregue com sucesso para: {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Falha ao entregar a mensagem: {ex.Error.Reason}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(1));
            _producer.Dispose();
        }
    }
}

