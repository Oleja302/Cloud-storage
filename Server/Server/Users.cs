namespace Server
{
    internal class Users
    {
        public List<Client> Clients { get; private set; } = new List<Client>();
        public List<Admin> Admins { get; private set; } = new List<Admin>();

        public void AddClient(Account account)
        {
            Clients.Add(new Client(account));
        }

        public void AddAdmin(Account account)
        {
            Admins.Add(new Admin(account));
        }

        public void AddClient(Client c)
        {
            Clients.Add(c);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{c.account.Email}");
        }

        public void AddAdmin(Admin a)
        {
            Admins.Add(a);
        }

        public void CheckClientOrAdmin(Account acc, ref bool checkClient, ref bool checkAdmin)
        {
            //Check client
            foreach (Client c in this.Clients)
                if (acc.Email == c.account.Email && acc.Password == c.account.Password)
                {
                    checkClient = true;
                    return;
                }

            //Check admin
            foreach (Admin a in this.Admins)
                if (acc.Email == a.account.Email && acc.Password == a.account.Password)
                {
                    checkAdmin = true;
                    return;
                }
        }

        public void ReadUsersFromFile()
        {
            string[] userData;

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\admins.txt"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\admins.txt").Close();

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Close();

            //Read clients
            if (new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Length != 0)
                using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
                    while (!reader.EndOfStream)
                    {
                        userData = reader.ReadLine().Split(" ");
                        Client c = new Client(new Account(userData[0], userData[1], userData[2]));
                        c.CalculateUsedSpace();
                        AddClient(c);
                    }

            //Read admins
            if (new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\admins.txt").Length != 0)
                using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\admins.txt"))
                    while (!reader.EndOfStream)
                    {
                        userData = reader.ReadLine().Split(" ");
                        Admin a = new Admin(new Account(userData[0], userData[1], userData[2]));
                        AddAdmin(a);
                    }
        }

        public void WriteClientsToFile()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Close();

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt", false))
                foreach (Client user in Clients)
                    writer.WriteLine($"{user.account.Email} {user.account.Password} {user.account.Name}");
        }

        public Client GetClient(string email, string password)
        {
            foreach (Client c in Clients)
                if (c.account.Email == email && c.account.Password == password) return c;

            return null;
        }
    }
}
