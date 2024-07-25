using System.Threading.Tasks;
using NLog;

namespace core
{
    /// <summary>
    /// ИУС менеджер, в котором содержится логика исполнения утилиты
    /// </summary>
    public class IusManager {        
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private DataBaseManager _DataBaseManager = new DataBaseManager();
        private FileManager _fileManager = new FileManager();
        private ArtifactManager _ArtifactManager = new ArtifactManager();
        private NpmManager _NpmManager = new NpmManager();
        private NugetManager _NugetManager = new NugetManager();
        public void utility_core(LocalConfiguration config)
        {
            Constants.combineAllPath(config.informIusPath);
            _fileManager.copyAndReplaceConfig(config.nginxConfPath, config.param);

            _ArtifactManager.taskKill(Constants.webApi, config.processWaitingTime);

            var backupResoreUpdateBd = Task.Factory.StartNew(() =>
            {
                _DataBaseManager.backupAndRestore(config.DB_BackupPath, config.processWaitingTime);
                _DataBaseManager.updateDB(config.informIusPath, config.dbname, config.processWaitingTime);
            });

            var removeBinObj = Task.Factory.StartNew(() =>
            {
                if (config.removeBinObj.Value)
                {
                    _ArtifactManager.removeBinObj(config.informIusPath, config.processWaitingTime);
                }
            });

            var removeAndRestoreNuget = Task.Factory.StartNew(() =>
            {
                if (config.removeNuget.Value)
                {
                    _ArtifactManager.removeNuget(config.cachePath);   
                    _logger.Info("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    _NugetManager.restoreNuget(config.nugetExePath, config.processWaitingTime);
                }
            });

            var rempveNodeModulesNPMI = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    _NpmManager.removeNodeModules(Constants.nodeModules);
                    _logger.Info("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    _NpmManager.removePackageLock(Constants.packageLock);
                    _logger.Info("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    _NpmManager.npmRestore(Constants.webApi, config.processWaitingTime);
                }
            });

            Task.WaitAll(
                backupResoreUpdateBd, removeBinObj,
                removeAndRestoreNuget, rempveNodeModulesNPMI
                );
        }
    }
}
