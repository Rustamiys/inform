using System.Threading.Tasks;
using NLog;

namespace appManager
{
    /// <summary>
    /// ИУС менеджер, в котором содержится логика исполнения утилиты
    /// </summary>
    public class IusManager {        
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private TaskManager _taskManager = new TaskManager();
        private FileManager _fileManager = new FileManager();
        public void utility_core(LocalConfiguration config)
        {
            Constants.combineAllPath(config.informIusPath);
            _fileManager.copyAndReplaceConfig(config.nginxConfPath, config.param);
            var taskKill = Task.Factory.StartNew(() =>
            _taskManager.taskKill(Constants.webApi, config.processWaitingTime)
            );
            taskKill.Wait();

            var backupResoreUpdateBd = Task.Factory.StartNew(() =>
            {
                _taskManager.backupAndRestore(config.DB_BackupPath, config.processWaitingTime);
                _taskManager.updateDB(config.informIusPath, config.dbname, config.processWaitingTime);
            });

            var removeBinObj = Task.Factory.StartNew(() =>
            {
                if (config.removeBinObj.Value)
                {
                    _taskManager.removeBinObj(config.informIusPath, config.processWaitingTime);
                }
            });

            var removeAndRestoreNuget = Task.Factory.StartNew(() =>
            {
                if (config.removeNuget.Value)
                {
                    _taskManager.removeNuget(config.cachePath);   
                    _logger.Info("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    _taskManager.restoreNuget(config.nugetExePath, config.processWaitingTime);
                }
            });

            var rempveNodeModulesNPMI = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    _taskManager.removeNodeModules(Constants.nodeModules);
                    _logger.Info("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    _taskManager.removePackageLock(Constants.packageLock);
                    _logger.Info("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    _taskManager.npmRestore(Constants.webApi, config.processWaitingTime);
                }
            });

            Task.WaitAll(
                backupResoreUpdateBd, removeBinObj,
                removeAndRestoreNuget, rempveNodeModulesNPMI
                );
        }
    }
}
