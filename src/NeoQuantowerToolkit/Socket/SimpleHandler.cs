using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Socket
{
    //📝 TODO: [split in server and client side]
    //📝 TODO: [add logger]

    public enum SocketCommand
    {
        ShareDataStructure,
        ShareConfiguration,
        PushData
    }
    public class SimpleHandler
    {
        public SocketManager Socket { get; set; }
        public Func<List<string>> ShareStructure { get; set; }

        public SimpleHandler(SocketManager socket, Func<List<string>> shareStructure)
        {
            Socket = socket;
            Socket.MessageReceived += this.Socket_MessageReceived;
            ShareStructure = shareStructure;
        }

        private void Socket_MessageReceived(object sender, StreaMessage e)
        {
            if (e.Command == SocketCommand.ShareDataStructure.ToString())
            {
                var list = this.ShareStructure?.Invoke() ?? new List<string>();

                var msg = new StreaMessage
                {
                    Command = SocketCommand.ShareDataStructure.ToString(),
                    Payload = string.Join(";", list)
                };



                var stream = Socket.TcpClients[0].GetStream();

                if (stream != null)
                    SocketManager.SendMessageAsync(stream, msg, CancellationToken.None).Wait();
                else
                    Console.WriteLine("⚠️ Stream non disponibile.");
            }



            //📝 TODO: [Retrive And Send Config]
            if (e.Command == SocketCommand.ShareConfiguration.ToString())
            {
                var msg = new StreaMessage
                {
                    Command = SocketCommand.ShareConfiguration.ToString(),
                    Payload = "Configurazione"
                };

                var stream = Socket.TcpClients[0].GetStream();

                if (stream != null)
                    SocketManager.SendMessageAsync(stream, msg, CancellationToken.None).Wait();
                else
                    Console.WriteLine("⚠️ Stream non disponibile.");
            }


            //📝 TODO: [Look for table, push data]

            if (e.Command == SocketCommand.PushData.ToString())
            {
                var msg = new StreaMessage
                {
                    Command = SocketCommand.PushData.ToString(),
                    Payload = "Push Data"
                };

                var stream = Socket.TcpClients[0].GetStream();

                if (stream != null)
                    SocketManager.SendMessageAsync(stream, msg, CancellationToken.None).Wait();
                else
                    Console.WriteLine("⚠️ Stream non disponibile.");
            }

        }
    }
}
