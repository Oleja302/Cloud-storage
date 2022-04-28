namespace Client
{
    internal interface IQuery
    {
        void Send(int count, long size);
        void Download(int count, long size);
        void Delete(int count);
    }
}
