using System;
using appManager;

namespace gitUpdateAutomation
{
    internal class Program
    {
        static void Main()
        {
            string localConfPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Git Update Auto/localConf.json";

            LocalConfiguration config = ConfigManager.getConfig(localConfPath);
            if (config == null)
            {
                return;
            }
            ConfigManager.setValuesIfNull(ref config);
            IusManager.managering(config);
        }
    }
}
