using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Chain
{
    class Program
    {
        private static Socket _listener;
        private static Socket _sender;

        private static void CreateNode(string listeningPort, string nextHost, string nextPort) 
        {   
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(listeningPort));
            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            _listener.Bind(localEndPoint);
            _listener.Listen(10);

            ipAddress = (nextHost == "localhost") ? IPAddress.Loopback :
                IPAddress.Parse(nextHost);

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Convert.ToInt32(nextPort));

            _sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tryToConnectSocket(remoteEP); 
        }

        static void tryToConnectSocket(IPEndPoint remoteEP) 
        {
            var result = _sender.BeginConnect(remoteEP, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(5000, true);

            if (!_sender.Connected)
            {
                _sender.EndConnect(result);
                throw new Exception("Cant connect to the socket");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CreateNode(args[0], args[1], args[2]);

                string consoleVariable = Console.ReadLine();

                if (args.Length == 3) 
                {
                    ActLikeUsualNode(consoleVariable);
                }
                else if (args.Length == 4 && args[3] == "true")
                {
                    ActLikeInitializer(consoleVariable);
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

        private static void ActLikeInitializer(string x)
        {
            _sender.Send(Encoding.UTF8.GetBytes(x));
            Socket handler = _listener.Accept();

            byte[] buf = new byte[4];
            int bytesRec = handler.Receive(buf);
            
            x = Encoding.UTF8.GetString(buf);

            _sender.Send(Encoding.UTF8.GetBytes(x));
            Console.WriteLine(x);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static void ActLikeUsualNode(string x)
        {
            Socket handler = _listener.Accept(); 

            string y;

            byte[] buf = new byte[4];
            int bytesRecFirst = handler.Receive(buf);

            y = Encoding.UTF8.GetString(buf);

            _sender.Send(Encoding.UTF8.GetBytes(Math.Max(Convert.ToInt32(x), Convert.ToInt32(y)).ToString()));

            handler.Receive(buf);

            _sender.Send(buf);

            Console.WriteLine(Encoding.UTF8.GetString(buf));

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

    }
}