using System;
using System.IO;
using System.Reflection.Metadata;
using appManager;
namespace gitUpdateAutomation
{
    internal class Program
    {        
        static void Main()
        {
            var iusManager = new IusManager();
            var configManager = new ConfigManager();
            var localConfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                , "Git Update Auto", "localConf.json");
            var config = configManager.getConfig(localConfPath);
            if (config == null)
            {
                return;
            }
            configManager.setValuesIfNull(ref config);
            iusManager.managering(config);
        }
    }
}
