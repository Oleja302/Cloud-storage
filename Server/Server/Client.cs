using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Client : IQuery
    {
        public Account account { get; private set; }
        public long UsedSpace { get; private set; }
        public bool VIP { get; private set; }

        public Client(Account a, int usedSpace = 0, bool vip = false)
        {
            this.account = a;
            this.UsedSpace = UsedSpace;
            this.VIP = vip;
        }

        public void ChangeUsedSpace(int size, bool increase)
        {
            if (increase) UsedSpace += size;
            else UsedSpace -= size;
        }

        public void SetVIP(bool vip)
        {
            VIP = vip;
        }

        public void Save(Socket handler)
        {
            string fullPath = string.Empty;
            int indName = 0;
            int indSize = 1;
            int allReceiveBytes = 0;
            int receiveBytes = 0;
            int receiveBytesWithoutName = 0;

            byte[] buffer = new byte[5242880];
            int[] bytesMap = new int[] { };
            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{account.Email}\\";

            //Get bytes map
            receiveBytes = handler.Receive(buffer);
            handler.Send(buffer);

            bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

            //Save file
            while (allReceiveBytes != bytesMap.Sum())
            {
                allReceiveBytes += handler.Receive(buffer, 0, bytesMap[indName], SocketFlags.None);
                fullPath = pathFilesClient + Encoding.Unicode.GetString(buffer, 0, bytesMap[indName]);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

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

            Logger l = new Logger();
            l.Save(bytesMap.Length, allReceiveBytes);
        }

        public void Download(Socket handler)
        {
            string fullPath = string.Empty;
            int receiveBytes = 0;
            string map = string.Empty;
            string savePath = string.Empty;
            List<string> filesName = new List<string>();
            long fileSize = 0;

            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{account.Email}\\";
            DirectoryInfo dirInfo = new DirectoryInfo(pathFilesClient);
            FileInfo[] cloudFiles = dirInfo.GetFiles();

            byte[] buffer = new byte[5242880];
            int[] bytesMap = new int[] { };

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
            foreach (string name in filesName)
            {
                if (File.Exists(pathFilesClient + name))
                {
                    map += name.Split("\\")[^1].Length * sizeof(char);
                    map += '.';
                    map += new FileInfo(pathFilesClient + name).Length;
                    map += '.';
                    fileSize += new FileInfo(pathFilesClient + name).Length;
                }
                else
                {
                    foreach (string f in Directory.GetFiles(pathFilesClient + name, "*", SearchOption.AllDirectories))
                    {
                        map += (f.Length - pathFilesClient.Length) * sizeof(char);
                        map += '.';
                        map += new FileInfo(f).Length;
                        map += '.';
                        fileSize += new FileInfo(f).Length;
                    }
                }
            }
            map = map.Remove(map.Length - 1);

            handler.Send(Encoding.Unicode.GetBytes(map));
            handler.Receive(buffer);

            //Send data
            foreach (string name in filesName)
            {
                if (File.Exists(pathFilesClient + name))
                {
                    handler.Send(Encoding.Unicode.GetBytes(name.Split("\\")[^1]));
                    handler.Send(File.ReadAllBytes(pathFilesClient + name));
                }
                else
                    foreach (string f in Directory.GetFiles(pathFilesClient + name, "*", SearchOption.AllDirectories))
                    {
                        handler.Send(Encoding.Unicode.GetBytes(f.Remove(0, f.IndexOf(account.Email) + account.Email.Length + 1)));
                        handler.Send(File.ReadAllBytes(f));
                    }
            }

            handler.Receive(buffer);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            Logger l = new Logger();
            l.Download(bytesMap.Length, fileSize);
        }

        public void Delete(Socket handler)
        {
            byte[] buffer = new byte[5242880];
            int[] bytesMap = new int[] { };

            int receiveBytes = 0;
            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{account.Email}\\";
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

            handler.Send(buffer);

            //Delete files
            foreach (string f in filesName)
            {
                if (File.Exists(pathFilesClient + f)) File.Delete(pathFilesClient + f);
                else Directory.Delete(pathFilesClient + f, true);
            }

            Logger l = new Logger();
            l.Delete(bytesMap.Length);
        }

        public void GetFiles(Socket handler)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\{account.Email}\\");
            string filesName = string.Empty;

            foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                filesName += file.FullName.Remove(0, file.FullName.IndexOf($"\\Storage\\{account.Email}\\") + $"\\Storage\\{account.Email}\\".Length) + "|";

            if (filesName.Length == 0)
                handler.Send(Encoding.Unicode.GetBytes("|"));

            else
            {
                filesName = filesName.Remove(filesName.Length - 1);
                handler.Send(Encoding.Unicode.GetBytes(filesName));
            }
        }
    }
}
