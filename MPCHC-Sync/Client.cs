using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPCHC_Sync
{

    public class ClientVideoEventArgs : EventArgs
    {
        public String file { get; set; }
        public TimeSpan position { get; set; }
        public State state { get; set; }
    }

    public class ClientConnectionEventArgs : EventArgs
    {
        public ConnectionState state { get; set; }
    }

    public class ClientErrorEventArgs : EventArgs
    {
        public int code { get; set; }
        public string description { get; set; }
    }

    public enum ConnectionState
    {
        Disconnected,
        Host,
        Subscribed
    }

    class Client
    {
        public ConnectionState connectionState { get; private set; }
        public string subscribedSessionIdentifer { get; private set; }
        public event EventHandler<ClientConnectionEventArgs> connectionStateChanged;
        public event EventHandler<ClientVideoEventArgs> videoStateChanged;
        public event EventHandler<ClientErrorEventArgs> onError;
        private TcpClient client;

        public bool Connect(String address, int port, bool host = true)
        {
            try
            {
                client = new TcpClient();
                client.Connect(address, port);
                NetworkStream stream = client.GetStream();
                new Thread(Read).Start();
                OnConnectionStateChanged(host ? ConnectionState.Host : ConnectionState.Subscribed);
            }catch (Exception e)
            {
                Debug.Write(e.Message);
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (client != null && client.Connected)
            {
                client.GetStream().Close();
                client.Close();
                OnConnectionStateChanged(ConnectionState.Disconnected);
            }
        }

        void Send(String msg)
        {
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(msg);

            try
            {
                client.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
            }
            catch
            {
                client.Close();
                OnConnectionStateChanged(ConnectionState.Disconnected);
            }
        }

        void Read()
        {
            NetworkStream stream = client.GetStream();
            Debug.WriteLine("Start read");
            while (client.Connected)
            {

                if (stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    string message = "";
                    int numberOfBytesRead = 0;

                    // Incoming message may be larger than the buffer size. 
                    do
                    {
                        try { 
                            numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        }catch
                        {
                            client.Close();
                            OnConnectionStateChanged(ConnectionState.Disconnected);
                        }
                        message += Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead);
                    }
                    while (stream.CanRead && stream.DataAvailable);

                    string[] commands = message.Split(new string[] { "<EOF>" }, StringSplitOptions.None);
                    for (int i = 0; i < commands.Length - 1; i++)
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
            OnConnectionStateChanged(ConnectionState.Disconnected);
        }

        void OnConnectionStateChanged(ConnectionState newState)
        {
            connectionState = newState;
            EventHandler<ClientConnectionEventArgs> handler = connectionStateChanged;
            if (handler != null)
            {
                ClientConnectionEventArgs args = new ClientConnectionEventArgs();
                args.state = newState;
                handler(this, args);
            }
        }

        public void Subscribe(string token, string identifer, bool host = false)
        {
            var data = new Dictionary<string, string>
            {
                ["token"] = token,
                ["identifer"] = identifer,
                ["command"] = host ? "host" : "subscribe"
            };
            subscribedSessionIdentifer = identifer;
            Send(JsonConvert.SerializeObject(data));
        }

        public void Set(string token, string identifer, string file, TimeSpan position, TimeSpan duration, State state)
        {
            var data = new Dictionary<string, string>
            {
                ["token"] = token,
                ["identifer"] = identifer,
                ["command"] = "set",
                ["file"] = file,
                ["position"] = position.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                ["duration"] = duration.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                ["state"] = ((int)state).ToString() 
            };
            Send(JsonConvert.SerializeObject(data));
        }

        public void Get(string token, string identifer)
        {
            var data = new Dictionary<string, string>
            {
                ["token"] = token,
                ["identifer"] = identifer,
                ["command"] = "get"
            };
            Send(JsonConvert.SerializeObject(data));
        }

        void ProcessResponce(String responceString)
        {
            Debug.WriteLine("Command: " + responceString);

            dynamic responce = JsonConvert.DeserializeObject(responceString);

            if(responce.status == "error")
            {
                Debug.WriteLine($"Error: {responce.description} code: {responce.code}");

                if (onError != null)
                {
                    ClientErrorEventArgs args = new ClientErrorEventArgs();
                    args.code = responce.code;
                    args.description = responce.description;
                    onError(this, args);
                }
            }

            if(responce.status == "ok")
            {
                Debug.WriteLine($"Status: {responce.status} Command: {responce.command}");
            }

            if(responce.new_data != null)
            {
                EventHandler<ClientVideoEventArgs> handler = videoStateChanged;
                if (handler != null)
                {
                    ClientVideoEventArgs args = new ClientVideoEventArgs();
                    args.file = responce.new_data.file;
                    args.position = TimeSpan.FromSeconds((double)responce.new_data.position);
                    args.state = responce.new_data.state;                   
                    handler(this, args);
                }
            }


        }
    }
}
