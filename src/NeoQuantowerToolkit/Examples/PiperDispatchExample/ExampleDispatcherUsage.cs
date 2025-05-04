using Neo.Quantower.Toolkit;
using System;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Examples.PiperDispatchExample
{
    public class MyMessage
    {
        public string Text { get; set; }
    }

    public static class ExampleDispatcherUsage
    {
        static Guid Tag = new Guid("00000000-0000-0000-0000-000000000001");
        public static async Task Run()
        {
            await PipeDispatcher.PipeDispatcher.Instance.Initialize();

            var subscription = PipeDispatcher.PipeDispatcher.Instance.Subscribe<MyMessage>(async message =>
            {
                Console.WriteLine($"[Handler] Received message: {message.Text}");
                await Task.CompletedTask;
            }, tag: Tag);

            await PipeDispatcher.PipeDispatcher.Instance.PublishAsync(new MyMessage
            {
                Text = "Hello from publisher!"
            });

            Console.WriteLine("Message published. Press Enter to exit.");
            Console.ReadLine();

            subscription.Dispose();
        }
    }
}

