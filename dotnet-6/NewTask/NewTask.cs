//
// Tutorial 2: Work Queues.  Create a Work Queue that will be used to distribute time-consuming tasks among multiple workers.
//

using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;

// The first parameter is the name of the exchange.
// The empty string denotes the default or nameless exchange:
// messages are routed to the queue with the name specified by routingKey, if it exists.
channel.BasicPublish(exchange: "", routingKey: "task_queue", basicProperties: properties, body: body);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}