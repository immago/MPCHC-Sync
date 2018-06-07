using System;
using System.IO;


namespace MPCHC_Sync
{
    class Settings
    {
        private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MPCHC-Sync");
        public string UUID;
        public string Token;

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
        }

    }
}
