//
// Tutorial 4: make it possible to subscribe only to a subset of the messages.
//

using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// changed type: "direct" to type: ExchangeType.Direct
channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

var severity = (args.Length > 0) ? args[0] : "info";
var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// 'Severity' is used as the routing key that determines to which queue the message is routed.
channel.BasicPublish(exchange: "direct_logs", routingKey: severity, basicProperties: null, body: body);
Console.WriteLine($" [x] Sent '{severity}':'{message}'");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();