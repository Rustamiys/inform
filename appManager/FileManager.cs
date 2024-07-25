using NLog;
using System.IO;


namespace appManager
{
    internal class FileManager
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        internal void copyAndReplaceConfig(string nginxConfPath, ParameterModel param)
        {
            // Копирование конфигурационных файлов WebApi на уровень выше и замена параметров
            configurationReplacement(Constants.configuration, nginxConfPath, Constants.nginx, param);
            configurationReplacement(Constants.configuration, Constants.webApi, Constants.ServiceConfiguration, param);
            configurationReplacement(Constants.configuration, nginxConfPath, Constants.upstreams, param);
            configurationReplacement(Constants.configuration, nginxConfPath, Constants.proxyServices, param);

            // Копирование экземпляров WebClient на уровень выше и замена параметров
            configurationReplacement(Constants.templates, Constants.webClient, Constants.envDevLocal, param);
            configurationReplacement(Constants.templates, Constants.webClient, Constants.envLocal, param);
        }
        private void configurationReplacement(string path, string pathMove, string filename, ParameterModel param)
        {
            path = Path.Combine(path, filename);
            pathMove = Path.Combine(pathMove, filename);
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                fileInf.CopyTo(pathMove, true);
                replaceParameterModel(pathMove, param);
            }
            else
            {
                _logger.Warn($"wrong path: {path}");
            }
        }
        private void replaceParameterModel(string path, ParameterModel param)
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
        internal void cleanFiles(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                _logger.Warn($"No such file or directory {path}");
            }
        }
        internal void cleanFiles(DirectoryInfo folder)
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
                _logger.Warn($"No such file or directory {folder.FullName}");
            }
        }
    }
}
