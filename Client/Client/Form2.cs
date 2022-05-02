using MaterialSkin.Controls;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form2 : MaterialForm
    {
        static int port = 8005;
        static string address = "127.0.0.1";
        bool input = false;

        public Form2()
        {
            InitializeComponent();
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (emailInput.Text == String.Empty || passwordInput.Text == string.Empty)
            {
                MessageBox.Show("Введены не все данные", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try { socket.Connect(ipPoint); }
            catch (SocketException)
            {
                MessageBox.Show("Не удалось подключиться к серверу", "Проблема с сервером", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] buffer = new byte[256];
            socket.Send(Encoding.Unicode.GetBytes($"{emailInput.Text} {passwordInput.Text}"));

            socket.Receive(buffer);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            if (BitConverter.ToInt32(buffer) == 1)
            {
                input = true;
                Close();
            }
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            if (emailReg.Text == String.Empty || nameReg.Text == string.Empty || passwordReg.Text == string.Empty)
            {
                MessageBox.Show("Введены не все данные", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try { socket.Connect(ipPoint); }
            catch (SocketException)
            {
                MessageBox.Show("Не удалось подключиться к серверу", "Проблема с сервером", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] buffer = new byte[256];
            socket.Send(Encoding.Unicode.GetBytes($"{emailReg.Text} {passwordReg.Text} {nameReg.Text}"));

            socket.Receive(buffer);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            if (BitConverter.ToInt32(buffer) == 0)
            {
                input = true;
                Close();
            }

            else MessageBox.Show("Вы уже зарегестрированы");
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!input)
                Application.Exit();
        }
    }
}
