using System;

namespace appManager
{
    public class LocalConfiguration
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
        public int closeWindowWaitingTime;
        public string dbname;
    }
    public class ParameterModel
    {
        public string PARAM_PORT_PREFIX;
        public string PARAM_DIST_PATH;
        public string PARAM_DATABASE;
        public string PARAM_VHOST;
    }
}
