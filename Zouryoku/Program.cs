using System.Text;

namespace Zouryoku
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                })
                .ConfigureLogging((host, configureLogging) =>
                {
                    // ログ設定
                    // 環境変数に応じて、log4netの設定ファイルを読み替える
                    var dotnetEnv = host.HostingEnvironment.EnvironmentName;
                    var log4netConfig = $"log4net.{dotnetEnv}.config";
                    if (!File.Exists(log4netConfig))
                    {
                        throw new FileNotFoundException(log4netConfig);
                    }
                    configureLogging.ClearProviders()
                        .AddLog4Net(log4netConfig, true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(serverOptions =>
                    {
                        // 1ファイルの最大容量50MBに設定
                        const int mb = 1024 * 1024;
                        serverOptions.Limits.MaxRequestBodySize = 50 * mb;
                    });
                });
    }
}
