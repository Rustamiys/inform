using System;
using System.IO;

namespace appManager
{
    internal class FileManagering
    {
        internal static void copyAndReplaceConfig(LocalConfiguration config)
        {
            // Копирование конфигурационных файлов WebApi на уровень выше и замена параметров
            configurationReplacement(config.informIusPath + "/WebApi/configuration", config.nginxConfPath, "nginx.conf", config.param);
            configurationReplacement(config.informIusPath + "/WebApi/configuration", config.informIusPath + "/WebApi", "ServiceConfiguration.json", config.param);
            configurationReplacement(config.informIusPath + "/WebApi/configuration", config.nginxConfPath, "upstreams.conf", config.param);
            configurationReplacement(config.informIusPath + "/WebApi/configuration", config.nginxConfPath, "proxy-services.conf", config.param);

            // Копирование экземпляров WebClient на уровень выше и замена параметров
            configurationReplacement(config.informIusPath + "/WebClient/teamplates", config.informIusPath + "/WebClient", ".env.dev.local", config.param);
            configurationReplacement(config.informIusPath + "/WebClient/teamplates", config.informIusPath + "/WebClient", ".env.local", config.param);
        }
        private static void configurationReplacement(string path, string pathMove, string filename, ParameterModel param)
        {
            path += "/" + filename;
            pathMove += "/" + filename;
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                fileInf.CopyTo(pathMove, true);
                replaceParameterModel(pathMove, param);
            }
            else
            {
                IusManager.logger.Warn($"wrong path: {path}");
            }
        }
        private static void replaceParameterModel(string path, ParameterModel param)
        {
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();

            content = content.Replace("%PARAM_PORT_PREFIX%", param.PARAM_PORT_PREFIX);
            content = content.Replace("%PARAM_DIST_PATH%", param.PARAM_DIST_PATH);
            content = content.Replace("%PARAM_DATABASE%", param.PARAM_DATABASE);
            content = content.Replace("%PARAM_VHOST%", param.PARAM_VHOST);

            StreamWriter writer = new StreamWriter(path);
            writer.Write(content);
            writer.Close();
        }
        internal static void cleanFiles(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                IusManager.logger.Warn($"No such file or directory {path}");
            }
        }
        internal static void cleanFiles(DirectoryInfo folder)
        {
            if (folder.Exists)
            {
                foreach (FileInfo file in folder.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in folder.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            else
            {
                IusManager.logger.Warn($"No such file or directory {folder.FullName}");
            }
        }
    }
}
