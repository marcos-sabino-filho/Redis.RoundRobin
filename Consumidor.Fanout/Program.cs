using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumidor.Fanout
{
	class Program
	{
		static void Main(string[] args)
		{

			/*
			 * ANTES DE CHAMAR ESSE EXE
			 * CHAMAR O EXE Produtor.Task
			 * ELE QUEM GERA OS ITENS NO EXCHANGE
			 * 
			 */
			Console.Write("Digite o nome da fila: ");
			string queueName = Console.ReadLine();

            var factory = new ConnectionFactory() { HostName = "localhost" };
			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();
			channel.QueueDeclare(queue: queueName,
								 durable: false,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);

			/*
			 * Pra começar a consumir com balance e round robin
			 * e não começar despejando as mensagens já existentes em apenas um consimidor
			 * https://www.rabbitmq.com/consumer-prefetch.html
			 */
			channel.BasicQos(0, 1, false);

			Console.WriteLine(" [*] Waiting for logs.");

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				try
				{
					var body = ea.Body.ToArray();
					var message = Encoding.UTF8.GetString(body);
					channel.BasicAck(ea.DeliveryTag, false);
					Console.WriteLine(" [x] {0}", message);
				}
				catch (Exception)
				{
					channel.BasicNack(ea.DeliveryTag, false, true);
				}
				System.Threading.Thread.Sleep(1000);
			};
			channel.BasicConsume(queue: queueName,
								 autoAck: false,
								 consumer: consumer);

			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();
		}
	}
}
