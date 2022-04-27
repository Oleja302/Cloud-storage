using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        static int port = 8005;
        static string address = "127.0.0.1";

        byte[] buffer = new byte[5242880];
        int[] bytesMap = new int[] { };
        static List<string> pathFiles = new List<string>();

        enum Query
        {
            Save = 1,
            Download = 2,
            Delete = 3
        }

        public Form1()
        {
            InitializeComponent();
        }

        void viewFileTree()
        {
            treeView1.Nodes.Clear();
            foreach (string path in pathFiles)
            {
                TreeNode node = new TreeNode();
                node.Text = Path.GetFileName(path);
                treeView1.Nodes.Add(node);
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            pathFiles.AddRange((string[])e.Data.GetData(DataFormats.FileDrop));
            pathFiles = pathFiles.Distinct().ToList();
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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                byte[] buffer = new byte[5242880];
                string map = string.Empty;

                //Send code query
                socket.Send(BitConverter.GetBytes((int)Query.Save));
                socket.Receive(buffer);

                //Send bytes map
                foreach (string path in pathFiles)
                {
                    map += Path.GetFileName(path).Length * sizeof(char);
                    map += '.';
                    map += new FileInfo(path).Length;
                    map += '.';
                }
                map = map.Remove(map.Length - 1);

                socket.Send(Encoding.Unicode.GetBytes(map));
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

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                int index = 0;
                int allReceiveBytes = 0;
                int receiveBytes = 0;
                int receiveBytesOneFile = 0;
                string fullPath = string.Empty;

                string map = string.Empty;
                string savePath = string.Empty;
                var filesName = new List<string>();

                //Send code query
                socket.Send(BitConverter.GetBytes((int)Query.Download));
                socket.Receive(buffer);

                //Get file name checked
                foreach (TreeNode node in treeView1.Nodes)
                    if (node.Checked) filesName.Add(node.Text);

                //Get path save
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    savePath = folderBrowserDialog1.SelectedPath;

                //Send bytes map
                foreach (string fn in filesName)
                {
                    map += fn.Length * sizeof(char);
                    map += '.';
                }
                map = map.Remove(map.Length - 1);

                socket.Send(Encoding.Unicode.GetBytes(map));
                socket.Receive(buffer);

                //Send file name 
                foreach (string name in filesName)
                    socket.Send(Encoding.Unicode.GetBytes(name));

                //Get bytes map
                receiveBytes = socket.Receive(buffer);
                socket.Send(buffer);

                bytesMap = Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('.').Select(x => int.Parse(x)).ToArray();

                //Save files
                while (allReceiveBytes != bytesMap.Sum())
                {
                    fullPath = savePath + '\\' + filesName[index];
                    for (int i = 1; receiveBytesOneFile != bytesMap[index]; i++)
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open(fullPath, FileMode.OpenOrCreate)))
                        {
                            if (buffer.Length >= bytesMap[index])
                            {
                                receiveBytes = socket.Receive(buffer, 0, bytesMap[index], SocketFlags.None);
                                writer.Write(buffer, 0, bytesMap[index]);
                            }
                            else
                            {
                                if (i * buffer.Length < bytesMap[index])
                                    receiveBytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                                else
                                    receiveBytes = socket.Receive(buffer, 0, bytesMap[index] - (i - 1) * buffer.Length, SocketFlags.None);
                                writer.Seek(receiveBytesOneFile, SeekOrigin.Begin);
                                writer.Write(buffer, 0, receiveBytes);
                            }

                            allReceiveBytes += receiveBytes;
                            receiveBytesOneFile += receiveBytes;
                        }
                    }

                    index++;
                    receiveBytesOneFile = 0;
                }

                socket.Send(buffer);
            }
            catch
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            string map = string.Empty;
            var filesName = new List<string>();

            //Send code query
            socket.Send(BitConverter.GetBytes((int)Query.Delete));
            socket.Receive(buffer);

            //Get file name checked
            foreach (TreeNode node in treeView1.Nodes)
                if (node.Checked) filesName.Add(node.Text);

            //Send bytes map
            foreach (string fn in filesName)
            {
                map += fn.Length * sizeof(char);
                map += '.';
            }
            map = map.Remove(map.Length - 1);

            socket.Send(Encoding.Unicode.GetBytes(map));
            socket.Receive(buffer);

            //Send file name 
            foreach (string name in filesName)
                socket.Send(Encoding.Unicode.GetBytes(name));

            ////Delete files from List<string> pathFile
            //foreach (string path in pathFiles)
            //    foreach (string name in filesName)
            //        if (path.Contains(name))
            //            pathFiles.Remove(path);

            socket.Receive(buffer);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            viewFileTree();
        }
    }
}