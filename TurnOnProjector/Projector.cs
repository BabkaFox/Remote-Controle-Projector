using System;
using System.Net;
using System.Net.Sockets;


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

        public abstract void turnOn();

        public abstract void turnOff();

        public abstract string getStatus();

        public abstract void freeze();

        public abstract String sendMessage(String message);
    }

}
