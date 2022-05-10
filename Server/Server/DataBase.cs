namespace Server
{
    internal class DataBase
    {
        public Users Users { get; private set; }

        public DataBase(Users users)
        {
            this.Users = users;
        }

        public long UsedSpaceClients { get; private set; } = 0;

        public void CalculateUsedSpaceClients()
        {
            UsedSpaceClients = 0;
            foreach (Client c in Users.Clients)            
                UsedSpaceClients += c.UsedSpace;           
        }
    }
}
