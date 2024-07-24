using System;
using appManager;

namespace gitUpdateAutomation
{
    internal class Program
    {        
        static void Main()
        {
            IusManager iusManager = new IusManager();
            ConfigManager configManager = new ConfigManager();
            string localConfPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Git Update Auto/localConf.json";

            LocalConfiguration config = configManager.getConfig(localConfPath);
            if (config == null)
            {
                return;
            }
            configManager.setValuesIfNull(ref config);
            iusManager.managering(config);
        }
    }
}
