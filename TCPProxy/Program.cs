using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPProxy
{
    class Program
    {
        private static TcpListener client2Proxy;
        private static TcpClient proxy2ServerConn;

        static void Main(string[] args)
        {
            Program.client2Proxy = new TcpListener(IPAddress.Any, 40123);
            Program.client2Proxy.Start();
            Console.WriteLine("Waiting for client connection on 40123...");
            TcpClient client2ProxyConn = Program.client2Proxy.AcceptTcpClient();
            Console.WriteLine("Client detected, proxying");

            proxy2ServerConn = new TcpClient("localhost", 40124);

            Thread proxy2Server = new Thread(() => Proxy2Server(proxy2ServerConn, client2ProxyConn));
            Thread client2Proxy = new Thread(() => Client2Proxy(client2ProxyConn, proxy2ServerConn));

            proxy2Server.Start();
            client2Proxy.Start();
        }

        private static void Client2Proxy(TcpClient client, TcpClient server)
        {
            NetworkStream clientStream = client.GetStream();
            NetworkStream serverStream = server.GetStream();

            int read = 0;
            byte[] buffer = new byte[4096];

            while (true)
            {
                try
                {
                    read = clientStream.Read(buffer, 0, 4096);
                    byte[] bufferCopy = new byte[read];
                    Array.Copy(buffer, bufferCopy, read);
                    if (read > 0)
                    {
                        Console.WriteLine("c->s [" + read + "]" + BitConverter.ToString(bufferCopy));
                        serverStream.Write(bufferCopy, 0, read);
                    }
                } catch (Exception e)
                {
                    Console.WriteLine("Error in Client2Proxy connection: " + e.Message);
                }


                Thread.Sleep(15);
            }
        }

        private static void Proxy2Server(TcpClient server, TcpClient client)
        {
            NetworkStream serverStream = server.GetStream();
            NetworkStream clientStream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[4096];
                int read = 0;
                try
                {
                    read = serverStream.Read(buffer, 0, 4096);
                    byte[] bufferCopy = new byte[read];
                    Array.Copy(buffer, bufferCopy, read);
                    if (read > 0)
                    {
                        Console.WriteLine("s->c [" + read + "] " + BitConverter.ToString(bufferCopy));
                        clientStream.Write(bufferCopy, 0, read);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in proxy2server connection: " + e.Message);
                }

                Thread.Sleep(15);
            }
        }
    }
}
