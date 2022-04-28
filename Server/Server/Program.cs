using System.Net;
using System.Net.Sockets;
using System.Text;

int port = 8005;
string address = "127.0.0.1";

string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + "\\Storage\\";
byte[] buffer = new byte[5242880];
int[] bytesMap = new int[] { };
var pathFiles = new List<string>();

void SaveData(Socket handler)
{
    string fullPath = string.Empty;
    int indName = 0;
    int indSize = 1;
    int allReceiveBytes = 0;
    int receiveBytes = 0;
    int receiveBytesWithoutName = 0;

    //Get bytes map
    receiveBytes = handler.Receive(buffer);
    handler.Send(buffer);

    bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

    //Save file
    while (allReceiveBytes != bytesMap.Sum())
    {
        allReceiveBytes += handler.Receive(buffer, 0, bytesMap[indName], SocketFlags.None);
        fullPath = pathFilesClient + Encoding.Unicode.GetString(buffer, 0, bytesMap[indName]);
        pathFiles.Add(fullPath);

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

void DownloadData(Socket handler)
{
    string fullPath = string.Empty;
    int receiveBytes = 0;
    string map = string.Empty;
    string savePath = string.Empty;
    List<string> filesName = new List<string>();

    //Get bytes map
    receiveBytes = handler.Receive(buffer);
    handler.Send(buffer);

    bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

    //Get files name
    foreach (int size in bytesMap)
    {
        receiveBytes = handler.Receive(buffer, 0, size, SocketFlags.None);
        filesName.Add(Encoding.Unicode.GetString(buffer, 0, receiveBytes));
    }

    //Send bytes map
    foreach (string path in pathFiles)
        foreach (string name in filesName)
            if (path.Contains(name))
            {
                map += new FileInfo(path).Length;
                map += '.';
            }

    map = map.Remove(map.Length - 1);

    handler.Send(Encoding.Unicode.GetBytes(map));
    handler.Receive(buffer);

    //Send data
    foreach (string path in pathFiles)
        foreach (string name in filesName)
            if (path.Contains(name))
                handler.Send(File.ReadAllBytes(path));

    handler.Receive(buffer);
    handler.Shutdown(SocketShutdown.Both);
    handler.Close();
}

void DeleteData(Socket handler)
{
    int receiveBytes = 0;
    List<string> filesName = new List<string>();
    DirectoryInfo dirInfo = new DirectoryInfo(pathFilesClient);

    //Get bytes map
    receiveBytes = handler.Receive(buffer);
    handler.Send(buffer);

    bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

    //Get files name
    foreach (int size in bytesMap)
    {
        receiveBytes = handler.Receive(buffer, 0, size, SocketFlags.None);
        filesName.Add(Encoding.Unicode.GetString(buffer, 0, receiveBytes));
    }

    //Delete files
    foreach (FileInfo file in dirInfo.GetFiles())
        if (filesName.Contains(file.Name))
            file.Delete();

    handler.Send(buffer);
}

IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

listenSocket.Bind(ipPoint);
listenSocket.Listen(10);

Console.WriteLine("Сервер запущен");

while (true)
{
    Socket clienSocket = listenSocket.Accept();
    clienSocket.Receive(buffer);
    clienSocket.Send(buffer);

    switch ((Query)BitConverter.ToInt32(buffer))
    {
        case Query.Save:
            SaveData(clienSocket);
            break;
        case Query.Download:
            DownloadData(clienSocket);
            break;
        case Query.Delete:
            DeleteData(clienSocket);
            break;
        default:
            break;
    }
}

enum Query
{
    Save = 1,
    Download = 2,
    Delete = 3
}