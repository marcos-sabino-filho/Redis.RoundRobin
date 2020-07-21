using RabbitMQ.Client;
using System;
using System.Text;

namespace Produtor.Task
{
	class Program
	{
		static void Main(string[] args)
		{
			string exchangeName = "exchangeParaFilas";
			var factory = new ConnectionFactory() { HostName = "localhost" };
			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();
			channel.QueueDeclare(queue: "pedidoRoundRobin", durable: false, exclusive: false, autoDelete: false, arguments: null);
			channel.QueueDeclare(queue: "log", durable: false, exclusive: false, autoDelete: false, arguments: null);
			channel.QueueDeclare(queue: "pdf", durable: false, exclusive: false, autoDelete: false, arguments: null);

			channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

			channel.QueueBind("pedidoRoundRobin", exchangeName, "");
			channel.QueueBind("log", exchangeName, "");
			channel.QueueBind("pdf", exchangeName, "");

			int contador = 0;
			while (true)
			{
				Console.WriteLine("");
				Console.Write("Tecle qualquer tecla para publicar 10 mensagens");
				Console.ReadLine();

				for (int i = 0; i < 10; i++)
				{
					string message = $"Pedido: {contador++}";
					var body = Encoding.UTF8.GetBytes(message);

					channel.BasicPublish(exchange: exchangeName,
										 routingKey: "",
										 basicProperties: null,
										 body: body);
					Console.WriteLine(" [x] Sent {0}", message);
				}
			}
		}
	}
}
