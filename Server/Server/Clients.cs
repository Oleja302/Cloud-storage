using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Clients
    {
        public List<Client> Users { get; private set; } = new List<Client>();
        public string Count { get; private set; }
        public void AddClient(Account account)
        {
            Users.Add(new Client(account));
        }
        public void ReadClientsFromFile()
        {
            string[] clientsData;

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
            {
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Close();
                return;
            }

            if (new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Length == 0) return;
            using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
            {
                while (!reader.EndOfStream)
                {
                    clientsData = reader.ReadLine().Split(" ");
                    AddClient(new Account(clientsData[0], clientsData[1], clientsData[2]));
                }
            }
        }

        public void WriteClientsToFile()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt").Close();

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\clients.txt", false))
            {
                foreach (Client user in Users)
                    writer.WriteLine($"{user.account.Email} {user.account.Password} {user.account.Name}");
            }
        }

        public Client GetClient(string email, string password)
        {
            foreach (Client c in Users)            
                if (c.account.Email == email && c.account.Password == password) return c;
            
            return null;
        }
    }
}
