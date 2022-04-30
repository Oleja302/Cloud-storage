using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;

int port = 8005;
string address = "127.0.0.1";

byte[] buffer = new byte[5242880];
DataBase db = new DataBase();

Client currentClient = null;
Admin admin = null;

IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

listenSocket.Bind(ipPoint);
listenSocket.Listen(10);

Console.WriteLine("Сервер запущен");
db.Clients.ReadClientsFromFile();

while (true)
{
    Socket clientSocket = listenSocket.Accept();

    if (currentClient == null && admin == null)
    {
        int receiveBytes = clientSocket.Receive(buffer);
        string[] dataUser = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split(' ');
        bool res = false;

        if (dataUser.Length == 2) res = db.CheckClient(new Account(dataUser[0], dataUser[1]));
        else res = db.CheckClient(new Account(dataUser[0], dataUser[1], dataUser[2]));

        if (dataUser.Length == 2)
        {
            if (res)
            {
                if (dataUser[0] == "admin@admin.com" && dataUser[1] == "111111")                
                    admin = new Admin(new Account(dataUser[0], dataUser[1], "admin"));              
                else             
                    currentClient = db.Clients.GetClient(dataUser[0], dataUser[1]);               

                clientSocket.Send(BitConverter.GetBytes(1));
            }
            else clientSocket.Send(BitConverter.GetBytes(0));
        }
        else if (dataUser.Length == 3)
        {
            if (!res)
            {
                db.Clients.AddClient(new Account(dataUser[0], dataUser[1], dataUser[2]));
                db.Clients.WriteClientsToFile();

                currentClient = new Client(new Account(dataUser[0], dataUser[1], dataUser[2]));
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{currentClient.account.Email}");

                clientSocket.Send(BitConverter.GetBytes(0));
            }
            else clientSocket.Send(BitConverter.GetBytes(1));
        }
    }
    else
    {
        clientSocket.Receive(buffer);

        switch ((Query)BitConverter.ToInt32(buffer))
        {
            case Query.Save:
                clientSocket.Send(buffer);
                if (currentClient != null) currentClient.Save(clientSocket);
                else admin.Save(clientSocket);
                break;
            case Query.Download:
                clientSocket.Send(buffer);
                if (currentClient != null) currentClient.Download(clientSocket);
                else admin.Download(clientSocket);
                break;
            case Query.Delete:
                clientSocket.Send(buffer);
                if (currentClient != null) currentClient.Delete(clientSocket);
                else admin.Delete(clientSocket);
                break;
            case Query.GetFiles:
                if (currentClient != null) currentClient.GetFiles(clientSocket);
                else admin.GetFiles(clientSocket);
                break;
            default:
                break;
        }
    }
}

enum Query
{
    Save = 1,
    Download = 2,
    Delete = 3,
    GetFiles = 4
}