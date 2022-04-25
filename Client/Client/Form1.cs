using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        string[] pathFiles;
        int port = 8005;
        string address = "127.0.0.1";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            byte[] buffer = new byte[] { };
            string bytesMap = string.Empty;

            //Send bytes map
            foreach (string s in pathFiles)
            {
                bytesMap += Path.GetFileName(s).Length * sizeof(char);
                bytesMap += '.';
                bytesMap += new FileInfo(s).Length;
                bytesMap += '.';
            }
            bytesMap = bytesMap.Remove(bytesMap.Length - 1);

            socket.Send(Encoding.Unicode.GetBytes(bytesMap));
            socket.Receive(buffer);

            //Send data and name file
            foreach (string path in pathFiles)
            {
                socket.Send(Encoding.Unicode.GetBytes(Path.GetFileName(path)));
                socket.Send(File.ReadAllBytes(path));
            }

            //socket.Receive(buffer);
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            pathFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
}