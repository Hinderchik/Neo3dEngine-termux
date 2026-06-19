using System.Net.Sockets;

namespace _3dEngine.Network;

public class NetworkConnection : IDisposable
{
    public Guid Id { get; }
    public TcpClient TcpClient { get; }
    public NetworkStream Stream { get; }
    public bool IsConnected => TcpClient.Connected;

    public NetworkConnection(TcpClient client, Guid id)
    {
        TcpClient = client;
        Id = id;
        Stream = client.GetStream();
    }

    public void SendPacket(byte[] data)
    {
        if (!IsConnected) return;

        byte[] length = BitConverter.GetBytes(data.Length);
        Stream.Write(length, 0, 4);
        Stream.Write(data, 0, data.Length);
    }

    public async Task<byte[]> ReceivePacketAsync()
    {
        byte[] lengthBuffer = new byte[4];
        int read = await Stream.ReadAsync(lengthBuffer, 0, 4);
        if (read != 4) return Array.Empty<byte>();

        int length = BitConverter.ToInt32(lengthBuffer, 0);
        if (length <= 0 || length > 1024 * 1024) return Array.Empty<byte>();

        byte[] data = new byte[length];
        int totalRead = 0;
        while (totalRead < length)
        {
            int chunk = await Stream.ReadAsync(data, totalRead, length - totalRead);
            if (chunk == 0) return Array.Empty<byte>();
            totalRead += chunk;
        }

        return data;
    }

    public void Dispose()
    {
        Stream?.Close();
        TcpClient?.Close();
    }
}
