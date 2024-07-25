using Newtonsoft.Json;
using System;
using System.IO;
using appManager;

namespace gitUpdateAutomation
{
    internal class ConfigManager
    {
        internal LocalConfiguration getConfig(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"No such file or directory {path}");
                return null;
            }
            var jsonString = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<LocalConfiguration>(jsonString);
        }
        internal void setValuesIfNull(ref LocalConfiguration config)
        {
            Console.Write($"Copy sorce database name: ");
            config.dbname = Convert.ToString(Console.ReadLine());
            if (!config.removeBinObj.HasValue)
            {
                setValues("remove-bin-obj", ref config.removeBinObj);
            }
            if (!config.removeNuget.HasValue)
            {
                setValues("remove nuget cache", ref config.removeNuget);
            }
            if (!config.removeNodeModules.HasValue)
            {
                setValues("remove node-modules", ref config.removeNodeModules);
            }
            if (!config.removePackageLock.HasValue)
            {
                setValues("remove package-lock", ref config.removePackageLock);
            }
            if (!config.recoveryNpmPackage.HasValue)
            {
                setValues("recovery npm package", ref config.recoveryNpmPackage);
            }
            if (!config.recoveryNugetPackage.HasValue)
            {
                setValues("recovery nuget pacckage", ref config.recoveryNugetPackage);
            }
        }
        private void setValues(string name, ref bool? param)
        {
            while (true)
            {
                Console.Write($"Use {name} (y/n): ");
                char ch = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (ch == 'y') { param = true; return; }
                if (ch == 'n') { param = false; return; }
                Console.WriteLine("wrong input");
            }
        }
    }
}
