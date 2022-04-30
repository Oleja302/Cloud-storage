namespace Server
{
    internal class Account
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }

        public Account(string email, string password, string name = "")
        {
            Email = email;
            Password = password;
            Name = name;
        }
    }
}
