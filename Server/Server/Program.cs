using System.Net;
using System.Net.Sockets;
using System.Text;

int port = 8005;

IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listenSocket.Bind(ipPoint);
listenSocket.Listen(10);

Console.WriteLine("Сервер запущен");

while (true)
{
    Socket handler = listenSocket.Accept();

    string fullPath = string.Empty;
    int indName = 0;
    int indSize = 1;
    int allReceiveBytes = 0;
    int receiveBytes = 0;
    int receiveBytesWithoutName = 0;
    byte[] buffer = new byte[5242880];
    int[] bytesMap = new int[] { };

    //Get bytes map
    receiveBytes = handler.Receive(buffer);
    handler.Send(buffer);

    bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

    //Save file
    while (allReceiveBytes != bytesMap.Sum())
    {
        allReceiveBytes += handler.Receive(buffer, 0, bytesMap[indName], SocketFlags.None);
        fullPath = @"D:\" + Encoding.Unicode.GetString(buffer, 0, bytesMap[indName]);

        for (int i = 1; receiveBytesWithoutName != bytesMap[indSize]; i++)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fullPath, FileMode.OpenOrCreate)))
            {
                if (buffer.Length >= bytesMap[indSize])
                {
                    receiveBytes = handler.Receive(buffer, 0, bytesMap[indSize], SocketFlags.None);
                    writer.Write(buffer, 0, bytesMap[indSize]);
                }
                else
                {
                    if (i * buffer.Length < bytesMap[indSize])
                        receiveBytes = handler.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    else
                        receiveBytes = handler.Receive(buffer, 0, bytesMap[indSize] - (i - 1) * buffer.Length, SocketFlags.None);
                    writer.Seek(receiveBytesWithoutName, SeekOrigin.Begin);
                    writer.Write(buffer, 0, receiveBytes);
                }

                allReceiveBytes += receiveBytes;
                receiveBytesWithoutName += receiveBytes;
            }
        }

        receiveBytesWithoutName = 0;
        indName += 2;
        indSize += 2;
    }

    handler.Send(buffer);
}