using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Produtor.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PedidoController : ControllerBase
	{
        private readonly ILogger<PedidoController> _logger;
        public PedidoController(ILogger<PedidoController> logger)
		{
            _logger = logger;
        }

        [HttpPost("gerarPedido")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GerarPedido()
		{
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();
			channel.QueueDeclare(queue: "pedidoRoundRobin",
								 durable: false,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);
			int contador = 0;
			while (true)
			{
				string message = $"Pedido: {contador++}";
				var body = Encoding.UTF8.GetBytes(message);

				channel.BasicPublish(exchange: "",
									 routingKey: "pedidoRoundRobin",
									 basicProperties: null,
									 body: body);
				_logger.LogTrace(" [x] Sent {0}", message);

				System.Threading.Thread.Sleep(2000);
			}

			//return Ok();
		}
	}
}
