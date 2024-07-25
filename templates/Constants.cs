using System.IO;

public class Constants
{
    public static string webApi = "WebApi";
    public static string webClient = "WebClient";
    public static string configuration = Path.Combine(webApi, "configuration");
    public static string templates = Path.Combine(webClient, "templates");
    public static readonly string ServiceConfiguration = "ServiceConfiguration.json";
    public static readonly string nginx = "nginx.conf";
    public static readonly string upstreams = "upstreams.conf";
    public static readonly string proxyServices = "proxy-services.conf";
    public static readonly string envDevLocal = ".env.dev.local";
    public static readonly string envLocal = ".env.local";
    public static string packageLock = Path.Combine(webApi, "package-lock.json");
    public static string nodeModules = Path.Combine(webApi, "node_modules");
    public static void combineAllPath(string path)
    {
        webApi = Path.Combine(path, webApi);
        webClient = Path.Combine(path, webClient);
        configuration = Path.Combine(path, configuration);
        templates = Path.Combine(path, templates);
        packageLock = Path.Combine(path, packageLock);
        nodeModules = Path.Combine(path, nodeModules);
    }
}
