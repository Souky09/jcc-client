using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharedClasses.Persistence
{
    public  class SocketClient
    {
        private static Socket socket;


        public  string SocketStart(string ip, int port,string content)
        {
            var response = "";
            try
            {
                if(Connect(ip, port))
                {
                    Send(content,ip,port);
                    response = Receive();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return response;
        }


        public bool Connect(string ip,int port)
        {
            bool status = false;
            IPAddress IpAddress;
            try
            {
                IPAddress.TryParse(ip, out IpAddress);
                //IPHostEntry host = Dns.GetHostEntry("localhost");
                //IpAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(IpAddress, port);
               
                socket = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
               

                socket.Connect(localEndPoint);
                status= socket.Connected;
            }
            catch(Exception e)
            {
                Log.Fatal("Error establishing the connection",e);
            }
            return status;
        }

        public bool Send(string content,string ip,int port)
        {
            var success = false;
            try
            {
                if(Connect(ip, port))
                {
                    if (socket.Connected && socket != null)
                    {
                        byte[] messageSent = Encoding.ASCII.GetBytes(content);
                    
                            int byteSent = socket.Send(messageSent);
                            Log.Fatal("bytesent!!!!!!!  " + byteSent);
                            success = true;
                       
                      
                    }
                }
                
            }
            catch(ArgumentNullException e)
            {
                Log.Fatal("send 1  " + e.Message);
            }
            catch (SocketException e)
            {
                Log.Fatal("send 2  " + e.Message);
            }
            catch (Exception e)
            {
                Log.Fatal("send 3  " + e.Message);
            }
            return success;
        }


        public string Receive()
        {
            byte[] messageReceived = new byte[1024];
            string responseString = null;
            try
            {
                if (socket.Connected && socket != null)
                {
                    socket.ReceiveTimeout = 30;
                    int byteRecv = socket.Receive(messageReceived);
                    responseString = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);

                }
            }
            catch (ArgumentNullException e)
            {
                Log.Fatal(e.Message);            }
            catch (SocketException e)
            {
                Log.Fatal(e.Message);
            }
            catch (Exception e)
            {
                Log.Fatal(e.Message);
            }
            Close();
            return responseString;

        }

        public void Close()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
