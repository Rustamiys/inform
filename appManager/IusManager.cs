using System.Threading.Tasks;
using NLog;

namespace appManager
{
    public class IusManager {
        private NLog.ILogger logger = LogManager.GetCurrentClassLogger();
        private ArtifactManager artifactmanager = new ArtifactManager();
        public void managering(LocalConfiguration config)
        {

            artifactmanager.taskKill(config.informIusPath + "/WebApi", config.processWaitingTime, config.closeWindowWaitingTime).Wait();

            Task backupResoreUpdateBd = Task.Factory.StartNew(() =>
            {
                artifactmanager.backupAndRestore(config.DB_BackupPath, config.processWaitingTime, config.closeWindowWaitingTime).Wait();
                artifactmanager.updateDB(config.informIusPath, config.dbname, config.processWaitingTime, config.closeWindowWaitingTime).Wait();
            });

            Task removeBinObj = Task.Factory.StartNew(() =>
            {
                if (config.removeBinObj.Value)
                {
                    artifactmanager.removeBinObj(config.informIusPath, config.processWaitingTime, config.closeWindowWaitingTime).Wait();
                }
            });

            Task removeAndRestoreNuget = Task.Factory.StartNew(() =>
            {
                if (config.removeNuget.Value)
                {
                    artifactmanager.removeNuget(config.cachePath).Wait();
                    logger.Info("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    artifactmanager.restoreNuget(config.nugetExePath, config.processWaitingTime, config.closeWindowWaitingTime).Wait();
                    //artifactmanager.restoreNuget(config.nugetExePath, config.informIusPath, config.processWaitingTime, config.closeWindowWaitingTime).Wait();
                }
            });

            Task rempveNodeModulesNPMI = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    artifactmanager.removeNodeModules(config.informIusPath + "/WebApi/node_modules").Wait();
                    logger.Info("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    artifactmanager.removePackageLock(config.informIusPath + "/WebApi/package-lock.json").Wait();
                    logger.Info("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    artifactmanager.npmRestore(config.informIusPath + "/WebApi", config.processWaitingTime, config.closeWindowWaitingTime).Wait();
                }
            });

            Task.WaitAll(
                backupResoreUpdateBd, removeBinObj,
                removeAndRestoreNuget, rempveNodeModulesNPMI
                );
        }
    }
}
