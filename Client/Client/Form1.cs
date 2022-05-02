using MaterialSkin.Controls;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : MaterialForm
    {
        static int port = 8005;
        static string address = "127.0.0.1";

        static byte[] buffer = new byte[5242880];
        static int[] bytesMap = new int[] { };
        static List<string> pathFiles = new List<string>();
        static List<string> fileNames = new List<string>();
        static List<TreeNode> selectedNode = new List<TreeNode>();
        static Logger l = new Logger();

        enum Query
        {
            Save = 1,
            Download = 2,
            Delete = 3,
            GetFiles = 4
        }

        public Form1()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = $"Количество выбранных файлов: {pathFiles.Count}";
        }

        void viewFileTree()
        {
            treeView1.Nodes.Clear();

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            int receiveBytes = 0;
            string filesName = string.Empty;

            //Send code query
            socket.Send(BitConverter.GetBytes((int)Query.GetFiles));

            //Get files name
            do
            {
                receiveBytes = socket.Receive(buffer);
                filesName += Encoding.Unicode.GetString(buffer, 0, receiveBytes);
            }
            while (socket.Available > 0);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            if (filesName.Length == 1) return;

            //Performance files in tree          
            TreeNode currentNode = new TreeNode();
            
            if (treeView1.TopNode == null) treeView1.Nodes.Add(currentNode);
            else currentNode = treeView1.TopNode;

            foreach (string pathFile in Encoding.Unicode.GetString(buffer, 0, receiveBytes).Split('|'))
            {
                foreach (string node in pathFile.Split('\\'))
                {
                    TreeNode newNode = new TreeNode(node);
                    newNode.Name = node;

                    if (currentNode.Nodes.Find(node, false).Length == 0)
                        currentNode.Nodes.Add(newNode);
                    currentNode = currentNode.Nodes.Find(node, false)[0];
                }

                currentNode = treeView1.Nodes[0];
            }

            treeView1.ExpandAll();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            foreach (string path in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                if (Directory.Exists(path))
                {
                    string root = new DirectoryInfo(path).Name;
                    foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        pathFiles.Add(f);
                        fileNames.Add(f.Remove(0, f.IndexOf(root)));
                    }
                }
                else
                {
                    pathFiles.Add(path);
                    fileNames.Add(Path.GetFileName(path));
                }
            }


            pathFiles = pathFiles.Distinct().ToList();
            toolStripStatusLabel1.Text = $"Количество выбранных файлов: {pathFiles.Count}";
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
            if (pathFiles.Count == 0) return;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            byte[] buffer = new byte[5242880];
            string map = string.Empty;
            long fileSize = 0;

            //Send code query
            socket.Send(BitConverter.GetBytes((int)Query.Save));
            socket.Receive(buffer);

            //Send bytes map
            for (int i = 0; i < fileNames.Count; i++)
            {
                map += fileNames[i].Length * sizeof(char);
                map += '.';
                map += new FileInfo(pathFiles[i]).Length;
                map += '.';

                //For log
                fileSize += new FileInfo(pathFiles[i]).Length;
            }

            map = map.Remove(map.Length - 1);

            socket.Send(Encoding.Unicode.GetBytes(map));
            socket.Receive(buffer);

            //Send data and name file
            for (int i = 0; i < fileNames.Count; i++)
            {
                socket.Send(Encoding.Unicode.GetBytes(fileNames[i]));
                socket.Send(File.ReadAllBytes(pathFiles[i]));
            }

            socket.Receive(buffer);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            //Log
            l.Send(pathFiles.Count, fileSize);
            //

            pathFiles.Clear();
            fileNames.Clear();
            toolStripStatusLabel1.Text = $"Количество выбранных файлов: {pathFiles.Count}";
            viewFileTree();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedNode.Count == 0) return;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            int indName = 0;
            int indSize = 1;
            int allReceiveBytes = 0;
            int receiveBytes = 0;
            int receiveBytesOneFile = 0;
            string fullPath = string.Empty;
            long fileSize = 0;

            TreeNode currentNode;
            string map = string.Empty;
            string savePath = string.Empty;
            var filesName = new List<string>();

            //Send code query
            socket.Send(BitConverter.GetBytes((int)Query.Download));
            socket.Receive(buffer);

            //Get file name checked
            foreach (TreeNode node in selectedNode)
            {
                fullPath += node.Text;
                currentNode = node;

                while (currentNode.Parent != treeView1.TopNode)
                {
                    fullPath = fullPath.Insert(0, currentNode.Parent.Text + '\\');
                    currentNode = currentNode.Parent;
                }

                filesName.Add(fullPath);
                fullPath = string.Empty;
            }

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
                allReceiveBytes += socket.Receive(buffer, 0, bytesMap[indName], SocketFlags.None);
                fullPath = savePath + "\\" + Encoding.Unicode.GetString(buffer, 0, bytesMap[indName]);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                for (int i = 1; receiveBytesOneFile != bytesMap[indSize]; i++)
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fullPath, FileMode.OpenOrCreate)))
                    {
                        if (buffer.Length >= bytesMap[indSize])
                        {
                            receiveBytes = socket.Receive(buffer, 0, bytesMap[indSize], SocketFlags.None);
                            writer.Write(buffer, 0, bytesMap[indSize]);
                        }
                        else
                        {
                            if (i * buffer.Length < bytesMap[indSize])
                                receiveBytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                            else
                                receiveBytes = socket.Receive(buffer, 0, bytesMap[indSize] - (i - 1) * buffer.Length, SocketFlags.None);
                            writer.Seek(receiveBytesOneFile, SeekOrigin.Begin);
                            writer.Write(buffer, 0, receiveBytes);
                        }

                        
                        allReceiveBytes += receiveBytes;
                        receiveBytesOneFile += receiveBytes;
                    }
                }

                fileSize += receiveBytesOneFile;
                receiveBytesOneFile = 0;
                indName += 2;
                indSize += 2;              
            }

            socket.Send(buffer);

            foreach (TreeNode node in selectedNode)
            {
                node.BackColor = Color.Transparent;
                node.Tag = "not selected";
            }
            selectedNode.Clear();

            //Log
            l.Download(filesName.Count, fileSize);
            //
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedNode.Count == 0) return;
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            string map = string.Empty;
            var filesNameForDelete = new List<string>();
            string fullPath = string.Empty;
            TreeNode currentNode;

            //Send code query
            socket.Send(BitConverter.GetBytes((int)Query.Delete));
            socket.Receive(buffer);

            //Get file name checked
            foreach (TreeNode node in selectedNode)
            {
                fullPath += node.Text;
                currentNode = node;

                while (currentNode.Parent != treeView1.TopNode)
                {
                    fullPath = fullPath.Insert(0, currentNode.Parent.Text + '\\');
                    currentNode = currentNode.Parent;
                }

                filesNameForDelete.Add(fullPath);
                fullPath = string.Empty;
            }

            //Send bytes map
            foreach (string fn in filesNameForDelete)
            {
                map += fn.Length * sizeof(char);
                map += '.';
            }
            map = map.Remove(map.Length - 1);

            socket.Send(Encoding.Unicode.GetBytes(map));
            socket.Receive(buffer);

            //Send file name 
            foreach (string name in filesNameForDelete)
                socket.Send(Encoding.Unicode.GetBytes(name));

            socket.Receive(buffer);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            //Log
            l.Delete(filesNameForDelete.Count);
            //

            selectedNode.Clear();
            viewFileTree();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            Hide();
            form2.ShowDialog();
            viewFileTree();
            Show();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Node == treeView1.TopNode) return;
            if (e.Node.BackColor == Color.Gainsboro)
            {
                e.Node.BackColor = Color.Transparent;
                e.Node.Tag = "not selected";
                selectedNode.Remove(e.Node);
            }
            else
            {
                e.Node.BackColor = Color.Gainsboro;
                e.Node.Tag = "selected";
                selectedNode.Add(e.Node);
            }
        }
    }
}