namespace Server
{
    internal class Logger
    {
        public void Save(int count, long size)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\log.log").Close();

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
                writer.WriteLine($"{DateTime.Now} Количество добавленных файлов: {count}. Общий размер: {size}");
        }

        public void Download(int count, long size)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\log.log").Close();

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
                writer.WriteLine($"{DateTime.Now} Количество скачанных файлов\\папок: {count}. Общий размер: {size}");
        }

        public void Delete(int count)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\log.log"))
                File.Create(AppDomain.CurrentDomain.BaseDirectory + "\\log.log").Close();

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\log.log", true))
                writer.WriteLine($"{DateTime.Now} Количество удаленных файлов\\папок: {count}");
        }
    }
}
