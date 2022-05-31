//
// Tutorial 1: Hello World.  Send and receive messages from a named queue
//

using RabbitMQ.Client;
using System.Text;


var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// The first parameter is the name of the exchange.
// The empty string denotes the default or nameless exchange:
// messages are routed to the queue with the name specified by routingKey, if it exists.
channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();