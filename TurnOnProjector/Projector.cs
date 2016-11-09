using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TurnOnProjector
{
    public abstract class Projector
    {
        private IPAddress _ip;

        public IPAddress IP
        {
            get { return _ip;  }
            set { _ip = value; }
        }

        public abstract Task<string> turnOn();

        public abstract Task<string> turnOff();

        public abstract Task<string> getStatus();

        public abstract void freeze();

        public abstract Task<string> sendMessage(string message);
    }

}
