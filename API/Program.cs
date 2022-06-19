using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using CLPortmapper;
using System.Diagnostics;
using System.Reflection;

namespace API
{
    public class Program
    {
        #region Fields

        private static ILog log = LogManager.Init().SetDumpMethod(DumpType.NoBuffer).SetLogDirectory(Path.Combine(Assembly.GetExecutingAssembly().Location));
        private static int port = 8256;

        #endregion Fields

        #region Private Methods

        private static void Main(string[] args)
        {
            log.Info("Welcome to Kestrel Template");
            log.Debug("All Rights Reserved - LFInteractive LLC. (c) 2020-2022");
            if (args.Length == 0)
            {
                PortHandler.AddToFirewall();
                log.Warn($"Automatic Port Forwarding is Enabled");
                log.Debug($"Opening Port {port}");
                PortHandler.OpenPort(port);
                Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder =>
                {
                    log.Info($"Starting server on port {port}");
                    builder.UseIISIntegration();
                    builder.UseContentRoot(Directory.GetCurrentDirectory());
                    builder.UseKestrel(options =>
                    {
                        options.ListenAnyIP(port);
                    });
                    builder.UseStartup<Startup>();
                    log.Info("Server is now running!");
                    new Process()
                    {
                        StartInfo = new()
                        {
                            FileName = $"http://127.0.0.1:{port}/api/template",
                            UseShellExecute = true,
                        }
                    }.Start();
                }).Build().Run();

                log.Info("Shutting Down");
                PortHandler.ClosePort(port);
            }
            else
            {
                if (args[0] == "--firewall")
                {
                    PortHandler.AddToFirewall();
                }
            }
        }

        #endregion Private Methods
    }

    internal class Startup
    {
        #region Public Methods

        public void Configure(IApplicationBuilder app, IWebHostEnvironment evn)
        {
            app.UseForwardedHeaders();
            app.UseMvc();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseDefaultFiles();
        }

        public void ConfigureServices(IServiceCollection service)
        {
            service.AddMvc(action =>
            {
                action.EnableEndpointRouting = false;
            });
        }

        #endregion Public Methods
    }
}