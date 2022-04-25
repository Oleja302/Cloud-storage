using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        List<string> pathFiles;
        //string[] pathFiles;
        int port = 8005;
        string address = "127.0.0.1";

        public Form1()
        {
            InitializeComponent();
        }

        void viewFileTree()
        {
            foreach (string path in pathFiles)
            {
                TreeNode node = new TreeNode();
                node.Name = Path.GetFileName(path);
                node.Text = Path.GetFileName(path);
                treeView1.Nodes.Add(node);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                byte[] buffer = new byte[5242880];
                string bytesMap = string.Empty;

                for (int i = 0; i < pathFiles.Count; i++)
                {
                    try
                    {
                        pathFiles.AddRange(Directory.GetFiles(pathFiles[i]));
                        pathFiles.Remove(pathFiles[i]);
                    }
                    catch { }
                }

                //Send bytes map
                foreach (string path in pathFiles)
                {
                    bytesMap += Path.GetFileName(path).Length * sizeof(char);
                    bytesMap += '.';
                    bytesMap += new FileInfo(path).Length;
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

                socket.Receive(buffer);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                viewFileTree();
            }
            catch
            {

            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            pathFiles = new List<string> { };
            pathFiles.AddRange((string[])e.Data.GetData(DataFormats.FileDrop));
            panel1.Visible = false;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            panel1.Visible = true;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            float width = (float)4.0;
            Pen pen = new Pen(SystemColors.ControlDark, width);
            pen.DashStyle = DashStyle.Dot;
            e.Graphics.DrawLine(pen, 0, 0, 0, panel.Height - 0);
            e.Graphics.DrawLine(pen, 0, 0, panel.Width - 0, 0);
            e.Graphics.DrawLine(pen, panel.Width - 1, panel.Height - 1, 0, panel.Height - 1);
            e.Graphics.DrawLine(pen, panel.Width - 1, panel.Height - 1, panel.Width - 1, 0);
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }
    }
}