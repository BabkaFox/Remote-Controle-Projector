using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TurnOnProjector
{
    class BenqProjector : Projector
    {
        public override void freeze()
        {
            throw new NotImplementedException();
        }

        public override string getStatus()
        {
            throw new NotImplementedException();
        }

        public override string sendMessage(string message)
        {
            try
            {

                TcpClient socket = new TcpClient(IP.ToString(), 4352);
                NetworkStream client = socket.GetStream();
                

                byte[] userMessage = new byte[message.Length + 1];
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(message), 0, userMessage, 0, message.Length);
                userMessage[userMessage.Length - 1] = 0x0d;

                client.Write(userMessage, 0, userMessage.Length);
                byte[] data = new byte[256];
                Int32 bytes = client.Read(data, 0, data.Length);
                bytes = client.Read(data, 0, data.Length);
                String responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Response: " + responseData);

                return responseData;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public override void turnOff()
        {
            sendMessage("%1POWR=0");
        }

        public override void turnOn()
        {
            sendMessage("%1POWR=1");
        }
    }
}
