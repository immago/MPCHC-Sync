using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPCHC_Sync
{

    class Client
    {
        private TcpClient client;

        public void Connect(String host, int port)
        {
            client = new TcpClient();
            client.Connect(host, port);
            NetworkStream stream = client.GetStream();

            Send("hello world 1");
            Read();
        }


        void Send(String msg)
        {
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(msg);
            client.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
        }

        void Read()
        {
            NetworkStream stream = client.GetStream();
            if (stream.CanRead)
            {
                byte[] myReadBuffer = new byte[1024];
                string message = "";
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                    message += Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead);
                }
                while (stream.DataAvailable);

                string[] commands = message.Split(new string[] { "<EOF>" }, StringSplitOptions.None);
                for(int i = 0; i < commands.Length-1; i++)
                {
                    ProcessResponce(commands[i]);
                }
                message = commands[commands.Length - 1];

            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
            } 
        }

        void ProcessResponce(String responceString)
        {
            Debug.WriteLine("Command: " + responceString);

            dynamic responce = JsonConvert.DeserializeObject(responceString);

            string status = responce.status;

            if(status == "error")
            {
                Debug.WriteLine($"Error: {responce.description} code: {responce.code}");
            }

            if(status == "ok")
            {
                Debug.WriteLine($"Commnd: {responce.command}");
            }

        }
    }
}
