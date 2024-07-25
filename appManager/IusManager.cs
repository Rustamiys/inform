using System.IO;
using System.Threading.Tasks;
using NLog;

namespace appManager
{
    public class IusManager {        
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private TaskManager TaskManager = new TaskManager();
        private FileManager FileManager = new FileManager();
        public void managering(LocalConfiguration config)
        {
            Constants.combineAllPath(config.informIusPath);
            FileManager.copyAndReplaceConfig(config.nginxConfPath, config.param);
            var taskKill = Task.Factory.StartNew(() =>
            TaskManager.taskKill(Constants.webApi)
            );
            taskKill.Wait();

            var backupResoreUpdateBd = Task.Factory.StartNew(() =>
            {
                TaskManager.backupAndRestore(config.DB_BackupPath);
                TaskManager.updateDB(config.informIusPath, config.dbname);
            });

            var removeBinObj = Task.Factory.StartNew(() =>
            {
                if (config.removeBinObj.Value)
                {
                    TaskManager.removeBinObj(config.informIusPath);
                }
            });

            var removeAndRestoreNuget = Task.Factory.StartNew(() =>
            {
                if (config.removeNuget.Value)
                {
                    TaskManager.removeNuget(config.cachePath);   
                    _logger.Info("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    TaskManager.restoreNuget(config.nugetExePath);
                }
            });

            var rempveNodeModulesNPMI = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    TaskManager.removeNodeModules(Constants.nodeModules);
                    _logger.Info("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    TaskManager.removePackageLock(Constants.packageLock);
                    _logger.Info("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    TaskManager.npmRestore(Constants.webApi);
                }
            });

            Task.WaitAll(
                backupResoreUpdateBd, removeBinObj,
                removeAndRestoreNuget, rempveNodeModulesNPMI
                );
        }
    }
}
