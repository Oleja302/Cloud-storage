namespace Client
{
    internal class Logger : IQuery
    {
        public void Send(int count, long size)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
            {
                var file = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log");
                file.Close();
            }

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
            {
                writer.WriteLine($"{DateTime.Now} Количество добавленных файлов: {count}. Общий размер: {size}");
            }
        }

        public void Download(int count, long size)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
            {
                var file = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log");
                file.Close();
            }

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
            {
                writer.WriteLine($"{DateTime.Now} Количество скачанных файлов: {count}. Общий размер: {size}");
            }
        }

        public void Delete(int count)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
            {
                var file = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log");
                file.Close();
            }

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
            {
                writer.WriteLine($"{DateTime.Now} Количество удаленных файлов: {count}");
            }
        }

        public void WriteFilesOnCloud(List<string> filesName)
        {
            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log", false))
            {
                foreach (string name in filesName)
                    writer.WriteLine(name);
            }
        }

        public List<string> ReadFilesOnCloud()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log"))
            {
                var file = File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log");
                file.Close();
                return new List<string>();
            }

            var filesName = new List<string>();
            using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\file_list.log"))
            {
                while (!reader.EndOfStream)
                    filesName.Add(reader.ReadLine());
            }

            return filesName;
        }
    }
}
