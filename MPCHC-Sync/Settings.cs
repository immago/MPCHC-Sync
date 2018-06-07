using System;
using System.IO;
using System.Net;

namespace MPCHC_Sync
{
    class Settings
    {
        private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MPCHC-Sync");
        public string UUID { get; private set; }
        public string Token { get; private set; }
        public int Port { get; private set; }
        public string Host { get; private set; }

        public Settings()
        {
            Directory.CreateDirectory(path);

            string uuidPath = Path.Combine(path, "UUID");
            if (!File.Exists(uuidPath))
            {
                File.WriteAllText(uuidPath, Guid.NewGuid().ToString());
            }

            this.UUID = File.ReadAllText(uuidPath);
            this.Token = "86de0ff4-3115-4385-b485-b5e83ae6b890";
            this.Port = 5000;
            this.Host = Dns.GetHostName();
        }

    }
}
