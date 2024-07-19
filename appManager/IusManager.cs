using System;
using System.IO;
using System.Threading.Tasks;

namespace appManager
{
    public class IusManager
    {
        public static void managering(LocalConfiguration config)
        {
            ArtifactManager.taskKill(config.informIusPath + "/WebApi", config.processWaitingTime).Wait();


            Task backupResoreUpdateBd = Task.Factory.StartNew(() =>
            {
                ArtifactManager.backupAndRestore(config.DB_BackupPath, config.processWaitingTime).Wait();
                ArtifactManager.updateDB(config.informIusPath, config.dbname, config.processWaitingTime).Wait();
            });

            Task removeBinObj = Task.Factory.StartNew(() =>
            {
                if (config.removeBinObj.Value)
                {
                    ArtifactManager.removeBinObj(config.informIusPath, config.processWaitingTime).Wait();
                }
            });

            Task removeAndRestoreNuget = Task.Factory.StartNew(() =>
            {
                if (config.removeNuget.Value)
                {
                    ArtifactManager.removeNuget(config.cachePath).Wait();
                    Console.WriteLine("All nuget cache removed");
                }
                if (config.recoveryNugetPackage.Value)
                {
                    ArtifactManager.restoreNuget(config.nugetExePath, config.informIusPath, config.processWaitingTime).Wait();
                }
            });

            Task rempveNodeModulesNPMI = Task.Factory.StartNew(() =>
            {
                if (config.removeNodeModules.Value)
                {
                    ArtifactManager.removeNodeModules(config.informIusPath + "/WebApi/node_modules").Wait();
                    Console.WriteLine("Node modules removed");
                }
                if (config.removePackageLock.Value)
                {
                    ArtifactManager.removePackageLock(config.informIusPath + "/WebApi/package-lock.json").Wait();
                    Console.WriteLine("Package-lock removed");
                }
                if (config.recoveryNpmPackage.Value)
                {
                    ArtifactManager.npmRestore(config.informIusPath + "/WebApi", config.processWaitingTime).Wait();
                }
            });


            Task.WaitAll(
                backupResoreUpdateBd, removeBinObj,
                removeAndRestoreNuget, rempveNodeModulesNPMI
                );
        }
    }
}
