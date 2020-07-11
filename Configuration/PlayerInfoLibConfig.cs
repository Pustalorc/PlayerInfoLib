using Pustalorc.Libraries.MySqlConnectorWrapper.Configuration;
using Rocket.API;

namespace PlayerInfoLibrary.Configuration
{
    public class PlayerInfoLibConfig : IRocketPluginConfiguration, IConnectorConfiguration
    {
        public string DatabaseAddress { get; set; }
        public ushort DatabasePort { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionStringExtras { get; set; }
        public string TableNamePlayers { get; set; }
        public string TableNameInstances { get; set; }
        public bool UseCache { get; set; }
        public double CacheRefreshRequestInterval { get; set; }
        public ulong CacheSize { get; set; }

        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabasePort = 3306;
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            ConnectionStringExtras = "";
            TableNamePlayers = "playerinfo";
            TableNameInstances = "playerinfo_instances";
            UseCache = true;
            CacheRefreshRequestInterval = 180000;
            CacheSize = 24;
        }
    }
}