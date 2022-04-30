namespace Server
{
    internal class DataBase
    {
        public Clients Clients { get; private set; } = new Clients();
        public string FreeSpace { get; private set; } 
        public string[] FormatData { get; private set; } 

        public bool CheckClient(Account acc)
        {
            foreach (Client c in Clients.Users)          
                if (acc.Email == c.account.Email && acc.Password == c.account.Password) return true;
            
            return false;
        }
    }
}
