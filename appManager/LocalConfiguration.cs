using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace appManager
{
    internal class LocalConfiguration
    {
        public string informIusPath;
        public string nginxConfPath;
        public string DB_BackupPath;
        public ParameterModel param;
        public bool? removeBinObj;
        public bool? removeNuget;
        public bool? removeNodeModules;
        public bool? removePackageLock;
        public bool? recoveryNugetPackage;
        public bool? recoveryNpmPackage;
        public string[] cachePath;
        public string nugetExePath;
        public TimeSpan processWaitingTime;
        public void setValuesIfNull()
        {          
            if (!this.removeBinObj.HasValue) 
            { 
                setValues("remove-bin-obj", ref this.removeBinObj);
            }
            if (!this.removeNuget.HasValue) {
                setValues("remove nuget cache", ref this.removeNuget);
            }
            if (!this.removeNodeModules.HasValue) {
                setValues("remove node-modules", ref this.removeNodeModules);
                    }
            if (!this.removePackageLock.HasValue) {
                setValues("remove package-lock", ref this.removePackageLock);
            }
            if (!this.recoveryNpmPackage.HasValue) {
                setValues("recovery npm package", ref this.recoveryNpmPackage);
            }
            if (!this.recoveryNugetPackage.HasValue) {
                setValues("recovery nuget pacckage", ref this.recoveryNugetPackage);
            }
        }
        private void setValues(string name, ref bool? param)
        {
            while (true)
            {
                Console.Write($"Use {name} (y/n): ");
                string ch = Convert.ToString(Console.ReadLine());
                if (ch == "y") { param = true; return; }
                if (ch == "n") { param = false; return; }
                Console.WriteLine("wrong input");
            }
        }
    }
    internal class ParameterModel
    {
        public string PARAM_PORT_PREFIX;
        public string PARAM_DIST_PATH;
        public string PARAM_DATABASE;
        public string PARAM_VHOST;
    }
}
