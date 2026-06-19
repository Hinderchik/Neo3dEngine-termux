using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace _3dEngine.Network;

public class NetworkManager
{
    private TcpClient _client;
    private TcpListener _server;
    private NetworkStream _stream;
    private readonly ConcurrentQueue<(INetworkPacket packet, int senderId)> _packetQueue = new();

    private readonly List<NetworkConnection> _clients = new();
    private readonly object _clientsLock = new();

    public bool IsServer { get; private set; }

    public void StartServer(int port)
    {
        IsServer = true;
        _server = new TcpListener(IPAddress.Any, port);
        _server.Start();
        Task.Run(AcceptClientsAsync);
    }

    public void Connect(string ip, int port)
    {
        IsServer = false;
        _client = new TcpClient();
        _client.Connect(ip, port);
        _stream = _client.GetStream();
        Task.Run(ReceiveLoopAsync);
    }

    public void SendPacket<T>(T packet, int senderId) where T : INetworkPacket
    {
        byte[] finalBytes = SerializePacket(packet, senderId);

        if (IsServer)
        {
            Broadcast(finalBytes, senderId);
        }
        else if (_stream != null)
        {
            try { _stream.Write(finalBytes); } catch { }
        }
    }

    private byte[] SerializePacket<T>(T packet, int senderId) where T : INetworkPacket
    {
        int packetTypeId = PacketManager.GetId(packet.GetType());

        using var packetMs = new MemoryStream();
        using var packetWriter = new BinaryWriter(packetMs);
        packet.Serialize(packetWriter);
        byte[] packetData = packetMs.ToArray();

        using var finalMs = new MemoryStream();
        using var writer = new BinaryWriter(finalMs);

        writer.Write(packetTypeId);
        writer.Write(senderId);
        writer.Write(packetData.Length);
        writer.Write(packetData);

        return finalMs.ToArray();
    }

    private void Broadcast(byte[] data, int senderId)
    {
        lock (_clientsLock)
        {
            foreach (var connection in _clients.ToArray())
            {
                try
                {
                    connection.SendPacket(data);
                }
                catch
                {
                    _clients.Remove(connection);
                    connection.Dispose();
                }
            }
        }
    }

    private async Task ReceiveLoopAsync()
    {
        byte[] header = new byte[12];

        while (_client != null && _client.Connected)
        {
            try
            {
                int read = await _stream.ReadAsync(header, 0, 12);
                if (read == 0) break;

                using var ms = new MemoryStream(header);
                using var reader = new BinaryReader(ms);

                int typeId = reader.ReadInt32();
                int senderId = reader.ReadInt32();
                int length = reader.ReadInt32();

                byte[] data = new byte[length];
                int totalRead = 0;
                while (totalRead < length)
                {
                    totalRead += await _stream.ReadAsync(data, totalRead, length - totalRead);
                }

                INetworkPacket packet = PacketManager.CreateInstance(typeId);

                using var dataMs = new MemoryStream(data);
                using var dataReader = new BinaryReader(dataMs);
                packet.Deserialize(dataReader);

                _packetQueue.Enqueue((packet, senderId));
            }
            catch { break; }
        }
    }

    private async Task AcceptClientsAsync()
    {
        while (true)
        {
            try
            {
                var tcpClient = await _server.AcceptTcpClientAsync();
                var connection = new NetworkConnection(tcpClient, Guid.NewGuid());

                lock (_clientsLock)
                {
                    _clients.Add(connection);
                }

                _ = Task.Run(() => HandleClientAsync(connection));
            }
            catch { break; }
        }
    }

    private async Task HandleClientAsync(NetworkConnection connection)
    {
        try
        {
            while (connection.IsConnected)
            {
                byte[] data = await connection.ReceivePacketAsync();
                if (data.Length == 0) break;

                _packetQueue.Enqueue((ParsePacket(data), 0));
            }
        }
        finally
        {
            lock (_clientsLock)
            {
                _clients.Remove(connection);
            }
            connection.Dispose();
        }
    }

    private INetworkPacket ParsePacket(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        int typeId = reader.ReadInt32();
        int senderId = reader.ReadInt32();
        int length = reader.ReadInt32();

        byte[] packetData = reader.ReadBytes(length);

        INetworkPacket packet = PacketManager.CreateInstance(typeId);
        using var packetMs = new MemoryStream(packetData);
        using var packetReader = new BinaryReader(packetMs);
        packet.Deserialize(packetReader);

        return packet;
    }

    public void ProcessEvents()
    {
        while (_packetQueue.TryDequeue(out var item))
        {
            PacketManager.InvokeHandler(item.packet, item.senderId);
        }
    }
}
