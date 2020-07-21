using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Consumidor
{
	class Program
	{
		static void Main(string[] args)
		{
            var factory = new ConnectionFactory() { HostName = "localhost" };
			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();
			channel.QueueDeclare(queue: "pedidoRoundRobin",
								 durable: false,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);

			/*
			 * Pra começar a consumir com balance e round robin
			 * e não começar despejando as mensagens já existentes em apenas um consimidor
			 * https://www.rabbitmq.com/consumer-prefetch.html
			 */
			channel.BasicQos(0, 10, false);

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				try
				{
					var body = ea.Body.ToArray();
					var message = Encoding.UTF8.GetString(body);
					Console.WriteLine(" [x] Received {0}", message);

					/*
					 * https://www.rabbitmq.com/confirms.html
					 * PARA REMOVAR DA LISTA, APENAS QUANDO A REGRA DE NEGÓCIO ESTIVER OK
					 */
					channel.BasicAck(ea.DeliveryTag, false);
					System.Threading.Thread.Sleep(1000);
				}
				catch (Exception)
				{
					// PUBLICAR EM UMA FILA DE ERRO PARA NAO REPROCESSAR

					// VOLTA PRA LISTA SE HOUVER UM ERRO DURANTE O PROCESSAMENTO
					channel.BasicNack(ea.DeliveryTag, false, true);
				}
			};

			channel.BasicConsume(queue: "pedidoRoundRobin",
								 autoAck: false,
								 consumer: consumer);

			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();
		}
	}
}
