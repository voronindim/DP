using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace Client
{
    class Program
    {
        private const string EOF = "<EOF>";
        public static void StartClient(string address, int port, string message)
        {
            try
            {
                IPAddress ipAddress = address == "localhost" ? IPAddress.Loopback : IPAddress.Parse(address);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // CREATE
                Socket sender = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    // SEND
                    int bytesSent = sender.Send(Encoding.UTF8.GetBytes(message + EOF));

                    // RECEIVE
                    byte[] buf = new byte[1024];
                    string stringifiedHistory = "";
                    int bytesRec = sender.Receive(buf);
                    while (bytesRec != 0)
                    {
                        stringifiedHistory += Encoding.UTF8.GetString(buf, 0, bytesRec);
                        bytesRec = sender.Receive(buf);
                    }
                    var history = JsonSerializer.Deserialize<List<string>>(stringifiedHistory);
                    
                    foreach (var msg in history)
                    {
                        Console.WriteLine(msg);
                    }

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartClient(args[0], Int32.Parse(args[1]), args[2]);
        }
    }
}