using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TurnOnProjector
{
    class EpsonProjector : Projector
    {

        public static string getName(IPAddress ip)
        {
            String name = "";
            UdpClient sender = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(ip, 3629);
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

        public override async Task<string> getStatus()
        {
            string status = await sendMessage("PWSTATUS?");
            return status;
        }

        public override async Task<string> turnOn()
        {
            return await sendMessage("PWR ON");
        }

        public override async void freeze()
        {
            String status = await sendMessage("FREEZE?");
            Console.WriteLine(status);
            if (status.StartsWith("FREEZE=ON"))
                await sendMessage("FREEZE OFF");
            else
                await sendMessage("FREEZE ON");

        }

        public override async Task<string> turnOff()
        {
            return await sendMessage("PWR OFF");
        }

        public override async Task<string> sendMessage(string message)
        {
            try
            {
                TcpClient socket = new TcpClient(IP.ToString(), 3629);
                NetworkStream client = socket.GetStream();
                byte[] startMessage = { 0x45, 0x53, 0x43, 0x2f, 0x56, 0x50, 0x2e, 0x6e, 0x65, 0x74, 0x10, 0x03, 0x00, 0x00, 0x00, 0x00 };
                await client.WriteAsync(startMessage, 0, startMessage.Length);

                var by = new byte[2048];

                int bytesAvailable = await client.ReadAsync(by, 0, 2048);
                var responseData = System.Text.Encoding.ASCII.GetString(by, 0, bytesAvailable);

                //Console.WriteLine("Epson first answ "+responseData);

                byte[] userMessage = new byte[message.Length + 1];
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes(message), 0, userMessage, 0, message.Length);
                userMessage[userMessage.Length - 1] = 0x0d;

                await client.WriteAsync(userMessage, 0, userMessage.Length);
                client.Flush();

                by = new byte[2048];

                bytesAvailable = await client.ReadAsync(by, 0, 2048);
                responseData = System.Text.Encoding.ASCII.GetString(by, 0, bytesAvailable);

                //Console.WriteLine("Epson second answ " + responseData);


                socket.GetStream().Close();
                socket.Close();

                return responseData;
            }
            catch (Exception)
            {
                return "";
            }


        }

    }
}
