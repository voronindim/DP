using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace Chain
{
    class Program
    {
        private static Socket _listener;
        private static Socket _sender;

        private static void createConnection(int listenPort, string address, int port)
        {
            IPAddress listenIpAddress = IPAddress.Any;
            IPEndPoint localEP = new IPEndPoint(listenIpAddress, listenPort);
            _listener = new Socket(
                 listenIpAddress.AddressFamily,
                 SocketType.Stream,
                 ProtocolType.Tcp);

            _listener.Bind(localEP);
            _listener.Listen(10);

            IPAddress ipAddress = address == "localhost" ? IPAddress.Loopback : IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            _sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            connect(remoteEP);
        }

        static void connect(IPEndPoint remoteEP) 
        {
            bool isConnect = false;
            for (int i = 0; i < 3 && !isConnect; ++i)
            {
                try
                {
                    _sender.Connect(remoteEP);
                    isConnect = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }
            if (!isConnect)
            {
                throw new Exception("Cannot connect to next process");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                createConnection(Int32.Parse(args[0]), args[1], Int32.Parse(args[2]));

                int x = Convert.ToInt32(Console.ReadLine());

                if (args.Length == 3) 
                {
                    workAsUsualProcess(x);
                }
                else if (args.Length == 4 && args[3] == "true")
                {
                    workAsInitalizerProcess(x);
                }
                else
                {
                    throw new ArgumentException("Invalid argument");
                }

                _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void workAsInitalizerProcess(int x)
        {
            _sender.Send(BitConverter.GetBytes(x));

            Socket socket = _listener.Accept();

            byte[] buf = new byte[4];
            socket.Receive(buf); 
            x = BitConverter.ToInt32(buf);

            Console.WriteLine(x);

            _sender.Send(BitConverter.GetBytes(x));

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static void workAsUsualProcess(int x)
        {
            Socket socket = _listener.Accept(); 

            byte[] buf = new byte[4];
            int bytesRecFirst = socket.Receive(buf);
            int y = BitConverter.ToInt32(buf);

            _sender.Send(BitConverter.GetBytes(Math.Max(x, y)));

            socket.Receive(buf);
            Console.WriteLine(BitConverter.ToInt32(buf));

            _sender.Send(buf);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

    }
}