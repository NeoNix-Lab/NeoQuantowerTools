using Neo.Quantower.Toolkit.Examples.PiperDispatchExample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Neo.Quantower.Toolkit.Socket
{

    //📝 TODO: [Make it more user frendly]


    /// <summary>
    /// A manager class that handles both TCP server and client roles with persistent socket communication.
    /// Supports JSON-serialized message exchange with length-prefixed framing.
    /// </summary>
    public class SocketManager : IDisposable
    {
        public TcpListener? Listener { get; private set; }
        public TcpClient? Client { get; private set; }
        public bool IsServer { get; private set; }
        public bool IsConnected => Client?.Connected ?? false;
        public IPAddress Ip { get; init; }
        public int Port { get; init; }
        public int MaxClient { get; init; } = 3;

        private readonly List<TcpClient> _tcpClients = new();
        private NetworkStream? _stream;
        private readonly CancellationToken _cancellationToken;
        private CancellationTokenSource _cts;
        public event EventHandler<StreaMessage>? MessageReceived;

        /// <summary>
        /// Initializes a new instance of the SocketManager.
        /// </summary>
        /// <param name="address">The IP address to bind or connect to.</param>
        /// <param name="port">The port to bind or connect to.</param>
        /// <param name="isServer">Set to true to act as server, false for client.</param>
        /// <param name="cts">Optional cancellation token for graceful shutdown.</param>
        /// <param name="maxClients">Maximum clients for the server (default 3).</param>
        public SocketManager(IPAddress address, int port, bool isServer, CancellationTokenSource? cts = null, int maxClients = 3)
        {
            Ip = address;
            Port = port;
            IsServer = isServer;
            this._cts = cts ?? new CancellationTokenSource(10000);
            _cancellationToken = cts.Token;
            MaxClient = maxClients;
        }


        //📝 TODO: [Add Server client init flexibility]

        /// <summary>
        /// Starts the socket manager based on the specified role (client or server).
        /// </summary>
        public async Task StartAsync()
        {
            if (IsServer)
                await StartServerAsync();
            else
                await ConnectAsync();
        }

        /// <summary>
        /// Starts the TCP server and begins accepting clients asynchronously.
        /// </summary>
        private async Task StartServerAsync()
        {
            if (Listener != null)
                throw new InvalidOperationException("Server is already started.");

            Listener = new TcpListener(Ip, Port);
            Listener.Start();
            Console.WriteLine($"Server listening on {Ip}:{Port}");

            while (!_cancellationToken.IsCancellationRequested)
            {
                var client = await Listener.AcceptTcpClientAsync(_cancellationToken);

                if (_tcpClients.Count >= MaxClient)
                {
                    Console.WriteLine("Max client limit reached. Rejecting new connection.");
                    client.Close();
                    continue;
                }

                _tcpClients.Add(client);
                _ = Task.Run(() => HandleClientAsync(client, _cancellationToken));
            }
        }

        /// <summary>
        /// Starts listening for messages on the client side.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ClientStartListeningrAsync(CancellationToken ct)
        {
            if (!IsServer)
            {
                try
                {
                    using var stream = Client.GetStream();
                    while (!ct.IsCancellationRequested)
                    {
                        var message = await ReceiveMessageAsync(stream);
                        if (message == null) break;
                        this.MessageReceived?.Invoke(this, message);
                        Console.WriteLine($"[MSG] {message.Command}: {message.Payload}");

                        var response = new StreaMessage { Command = "ACK", Payload = "Received" };
                        await SendMessageAsync(stream, response, ct);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with Server {Client.Client.RemoteEndPoint}: {ex.Message}");
                }
                finally
                {
                    Client.Close();
                    Console.WriteLine("[-] Client disconnected");
                }
            }
            
        }

        /// <summary>
        /// Handles an individual client connection and message loop.
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            Console.WriteLine($"[+] Connected: {client.Client.RemoteEndPoint}");

            try
            {
                using var stream = client.GetStream();
                while (!ct.IsCancellationRequested)
                {
                    var message = await ReceiveMessageAsync(stream);
                    if (message == null) break;
                    this.MessageReceived?.Invoke(this, message);
                    Console.WriteLine($"[MSG] {message.Command}: {message.Payload}");


                    //📝 TODO: [Handle Ping Pong]

                    //var response = new StreaMessage { Command = "ACK", Payload = "Received" };
                    //await SendMessageAsync(stream, response, ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                _tcpClients.Remove(client);
                client.Close();
                Console.WriteLine("[-] Client disconnected");
            }
        }

        /// <summary>
        /// Connects the client to the remote server.
        /// </summary>
        private async Task ConnectAsync()
        {
            Client = new TcpClient();
            await Client.ConnectAsync(Ip, Port);
            _stream = Client.GetStream();

            while (!this._cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveMessageAsync(_stream);
                if (message == null) break;
                this.MessageReceived?.Invoke(this, message);
                Console.WriteLine($"[MSG] {message.Command}: {message.Payload}");

                //📝 TODO: [Handle Ping Pong]

                //var response = new StreaMessage { Command = "ACK", Payload = "Received" };
                //await SendMessageAsync(_stream, response, this._cancellationToken);
            }

            Console.WriteLine("Connected to server.");
        }

        /// <summary>
        /// Sends a JSON message to the stream with a 4-byte length prefix.
        /// </summary>
        public static async Task SendMessageAsync(NetworkStream stream, StreaMessage msg, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(msg);
            var data = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(data.Length);

            await stream.WriteAsync(len, ct);
            await stream.WriteAsync(data, ct);
        }

        /// <summary>
        /// Receives a length-prefixed JSON message from the stream.
        /// </summary>
        public static async Task<StreaMessage?> ReceiveMessageAsync(NetworkStream stream)
        {
            byte[] lenBuffer = new byte[4];
            int read = await stream.ReadAsync(lenBuffer);
            if (read == 0) return null;

            int msgLength = BitConverter.ToInt32(lenBuffer, 0);
            byte[] msgBuffer = new byte[msgLength];
            read = await stream.ReadAsync(msgBuffer);
            if (read == 0) return null;

            var json = Encoding.UTF8.GetString(msgBuffer);
            return JsonSerializer.Deserialize<StreaMessage>(json);
        }

        public void Dispose()
        {
            if(this._cts != null)
            {
                this._cts.Cancel();
                this._cts.Dispose();
            }
            if (Client != null)
            {
                Client.Close();
                Client = null;
                Console.WriteLine("Client disconnected.");
            }
            if (Listener != null)
            {
                Listener.Stop();
                Listener = null;
                Console.WriteLine("Server stopped.");
            }

            foreach (var client in _tcpClients)
            {
                client.Close();
            }
            _tcpClients.Clear();
        }
    }
}