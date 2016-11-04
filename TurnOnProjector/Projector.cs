using System;
using System.Net.Sockets;


namespace TurnOnProjector
{
    public class Projector
    {
        private System.Net.IPAddress ip;

        public Projector(System.Net.IPAddress ip)
        {
            this.ip = ip;
        }

        public System.Net.IPAddress IP
        {
            get { return ip;  }
            set { ip = value; }
        }

        public string getStatus()
        {
            String status = sendMessage("PWSTATUS?");
            return status;
        }

        public static string getName(System.Net.IPAddress ip)
        {
            String name = "";
            UdpClient sender = new UdpClient();
            System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(ip, 3629);
            try
            {
                Byte[] sendBytes = new byte[] { 0x45, 0x53, 0x43, 0x2f, 0x56, 0x50, 0x2e, 0x6e, 0x65, 0x74, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00 };
                sender.Send(sendBytes, sendBytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                sender.Close();

            }
            finally
            {
                sender.Close();
            }
            sender.Close();

            UdpClient receivingUdpClient = new UdpClient(3629);
            receivingUdpClient.Client.SendTimeout = 4000;
            receivingUdpClient.Client.ReceiveTimeout = 4000;

            //System.Net.IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.WriteLine("Start getName");
                byte[] receiveBytes = receivingUdpClient.Receive(ref endPoint);
                
                // Преобразуем и отображаем данные
                string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine(" --> " + returnData.ToString());
                name = returnData.ToString().Replace("\0", "").Replace("ESC/VP.net", "");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                receivingUdpClient.Close();
                return name;
            }
            return name;

        }

        public void turnOn()
        {
            sendMessage("PWR ON");
        }

        public void freeze()
        {
            String status = sendMessage("FREEZE?");
            Console.WriteLine(status);
            if (status.StartsWith("FREEZE=ON"))
                sendMessage("FREEZE OFF");
            else
                sendMessage("FREEZE ON");

        }


        public void turnOff()
        {
            sendMessage("PWR OFF");
        }

        private String sendMessage(String message)
        {
            try
            {
                TcpClient socket = new TcpClient(this.ip.ToString(), 3629);
                NetworkStream client = socket.GetStream();
                byte[] startMessage = { 0x45, 0x53, 0x43, 0x2f, 0x56, 0x50, 0x2e, 0x6e, 0x65, 0x74, 0x10, 0x03, 0x00, 0x00, 0x00, 0x00 };
                client.Write(startMessage, 0, startMessage.Length);
                byte[] data = new byte[256];
                Int32 bytes = client.Read(data, 0, data.Length);
                String responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                byte[] userMessage = new byte[message.Length + 1];
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(message), 0, userMessage, 0, message.Length);
                userMessage[userMessage.Length - 1] = 0x0d;
                client.Write(userMessage, 0, userMessage.Length);
                data = new byte[256];
                bytes = client.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                return responseData;
            }
            catch (Exception)
            {
                return "";
            }

            
        }

    }

}
