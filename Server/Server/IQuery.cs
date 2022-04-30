using System.Net.Sockets;

namespace Server
{
    internal interface IQuery
    {
        void Save(Socket handler);
        void Download(Socket handler);
        void Delete(Socket handler);
    }
}
