//
// Tutorial 6: Remote procedure call (RPC). Run a function on a remote computer and wait for the result.
//

// A note on RPC
// Although RPC is a pretty common pattern in computing, it's often criticised. The problems arise when a
// programmer is not aware whether a function call is local or if it's a slow RPC. Confusions like that
// result in an unpredictable system and adds unnecessary complexity to debugging. Instead of simplifying
// software, misused RPC can result in unmaintainable spaghetti code.

// Bearing that in mind, consider the following advice:
// Make sure it's obvious which function call is local and which is remote.
// Document your system. Make the dependencies between components clear.
// Handle error cases. How should the client react when the RPC server is down for a long time?
// When in doubt avoid RPC. If you can, you should use an asynchronous pipeline - instead of RPC-like blocking, results are asynchronously pushed to a next computation stage.

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

public class RpcClient
{
    private const string QUEUE_NAME = "rpc_queue";

    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

    public RpcClient()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        // declare a server-named queue
        replyQueueName = channel.QueueDeclare(queue: "").QueueName;
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(
          consumer: consumer,
          queue: replyQueueName,
          autoAck: true);
    }

    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);

        channel.BasicPublish(
            exchange: "",
            routingKey: QUEUE_NAME,
            basicProperties: props,
            body: messageBytes);

        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        return tcs.Task;
    }

    public void Close()
    {
        connection.Close();
    }
}

public class Rpc
{
    public static void Main(string[] args)
    {
        Console.WriteLine("RPC Client");
        string n = args.Length > 0 ? args[0] : "30";
        Task t = InvokeAsync(n);
        t.Wait();

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static async Task InvokeAsync(string n)
    {
        var rpcClient = new RpcClient();

        Console.WriteLine(" [x] Requesting fib({0})", n);
        var response = await rpcClient.CallAsync(n.ToString());
        Console.WriteLine(" [.] Got '{0}'", response);

        rpcClient.Close();
    }
}