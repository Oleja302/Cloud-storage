using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Admin : IQuery
    {
        public Account account { get; private set; }

        public Admin(Account a) 
        {
            account = a;
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
            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + "\\Storage\\";

            //Get bytes map
            receiveBytes = handler.Receive(buffer);
            handler.Send(buffer);

            bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

            //Save file
            while (allReceiveBytes != bytesMap.Sum())
            {
                allReceiveBytes += handler.Receive(buffer, 0, bytesMap[indName], SocketFlags.None);
                fullPath = pathFilesClient + Encoding.Unicode.GetString(buffer, 0, bytesMap[indName]);

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

        public void Download(Socket handler)
        {
            string fullPath = string.Empty;
            int receiveBytes = 0;
            string map = string.Empty;
            string savePath = string.Empty;
            List<string> filesName = new List<string>();

            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + "\\Storage\\";
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
            foreach (FileInfo f in cloudFiles)
                foreach (string name in filesName)
                    if (f.Name == name)
                    {
                        map += new FileInfo(f.FullName).Length;
                        map += '.';
                    }

            map = map.Remove(map.Length - 1);

            handler.Send(Encoding.Unicode.GetBytes(map));
            handler.Receive(buffer);

            //Send data
            foreach (FileInfo f in cloudFiles)
                foreach (string name in filesName)
                    if (f.Name == name)
                        handler.Send(File.ReadAllBytes(f.FullName));

            handler.Receive(buffer);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        public void Delete(Socket handler)
        {
            byte[] buffer = new byte[5242880];
            int[] bytesMap = new int[] { };

            int receiveBytes = 0;
            string pathFilesClient = AppDomain.CurrentDomain.BaseDirectory + "\\Storage\\";
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

        public void GetFiles(Socket handler)
        {
            string filesName = string.Empty;
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + $"\\Storage\\", "*", SearchOption.AllDirectories);

            foreach (string file in files)
                filesName += Path.GetFileName(file) + "|";

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
