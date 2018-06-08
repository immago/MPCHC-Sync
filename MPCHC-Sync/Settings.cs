using System;
using System.IO;
using System.Net;

namespace MPCHC_Sync
{
    class Settings
    {
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MPCHC-Sync");

        // UUID
        private static string _UUID;
        public static string UUID
        {
            get {

                if (_UUID != null)
                {
                    return _UUID;
                }

                _UUID = Read("UUID", Guid.NewGuid().ToString());
                return _UUID;
            }
            set { _UUID = value; Write("UUID", value); }
        }

        // Token
        private static string _Token;
        public static string Token
        {
            get
            {
                if (_Token != null)
                {
                    return _Token;
                }

                _Token = Read("Token", "86de0ff4-3115-4385-b485-b5e83ae6b890");
                return _Token;
            }
            set { _Token = value; Write("Token", value); }
        }


        // Port
        private static int _Port;
        public static int Port
        {
            get
            {
                if (_Port > 0)
                {
                    return _Port;
                }

                _Port = Int32.Parse(Read("Port", "5000"));
                return _Port;
            }
            set { _Port = value; Write("Port", value.ToString()); }
        }

        // Host
        private static string _Host;
        public static string Host
        {
            get
            {
                if (_Host != null)
                {
                    return _Host;
                }

                _Host = Read("Host", Dns.GetHostName());
                return _Host;
            }
            set { _Host = value; Write("Host", value); }
        }

        // MPCWebUIAddress
        private static string _MPCWebUIAddress;
        public static string MPCWebUIAddress
        {
            get
            {
                if (_MPCWebUIAddress != null)
                {
                    return _MPCWebUIAddress;
                }

                _MPCWebUIAddress = Read("MPCWebUIAddress", "http://localhost:13579");
                return _MPCWebUIAddress;
            }
            set { _MPCWebUIAddress = value; Write("MPCWebUIAddress", value); }
        }

        public static bool IsConfigured()
        {
            return FileExists("Host") && FileExists("Port") && FileExists("Token") && FileExists("UUID") && FileExists("MPCWebUIAddress");
        }

        private static string Read(string variableName, string defaultValue)
        {
            Directory.CreateDirectory(path);
            string varPath = Path.Combine(path, variableName);
            if (!File.Exists(varPath))
            {
                File.WriteAllText(varPath, defaultValue);
            }
            return File.ReadAllText(varPath);
        }

        private static void Write(string variableName, string value)
        {
            Directory.CreateDirectory(path);
            string varPath = Path.Combine(path, variableName);
            File.WriteAllText(varPath, value); 
        }

        private static bool FileExists(string name)
        {
            Directory.CreateDirectory(path);
            return File.Exists(Path.Combine(path, name));
        }
    }
}
