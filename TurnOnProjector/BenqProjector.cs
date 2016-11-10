using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TurnOnProjector
{
    class BenqProjector : Projector
    {
        public override async void freeze()
        {
            Console.WriteLine(await sendMessage("%1FREZ ?"));
            await sendMessage("%1FREZ ?");
        }

        public override async Task<string> getStatus()
        {
            var stat = await sendMessage("%1POWR ?");
            Console.WriteLine(@"Функция getStatus: "+ stat);
            return stat;
        }

        public override async Task<string> sendMessage(string message)
        {
            try
            {
                TcpClient socket = new TcpClient(IP.ToString(), 4352);
                NetworkStream client = socket.GetStream();

                var by = new byte[2048];

                int bytesAvailable = await client.ReadAsync(by, 0, 2048);
                var msg = Encoding.ASCII.GetString(by, 0, bytesAvailable);

                byte[] userMessage = new byte[message.Length + 1];
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(message), 0, userMessage, 0, message.Length);
                userMessage[userMessage.Length - 1] = 0x0d;

                await client.WriteAsync(userMessage, 0, userMessage.Length);
                client.Flush();

                by = new byte[2048];

                bytesAvailable = await client.ReadAsync(by, 0, 2048);
                msg = Encoding.ASCII.GetString(by, 0, bytesAvailable);

                socket.GetStream().Close();
                socket.Close();

                return msg;

            }
            catch (Exception)
            {
                return "ERRORORO";
            }
        }

        public override async Task<string> turnOff()
        {
            return await sendMessage("%1POWR 0");
        }

        public override async Task<string> turnOn()
        {
            return await sendMessage("%1POWR 1");
        }
    }
}
