global using static OpenDSM.Lib.Data.Global;
using CLPortmapper;

namespace API;

public class Program
{
    #region Fields

    private static int port = 8080;

    #endregion Fields

    #region Private Methods

    private static void Main(string[] args)
    {
        if (args.Contains("--firewall"))
        {
            PortHandler.AddToFirewall();
        }
        else
        {
            log.Info("Welcome to OpenDSM Server");
            log.Debug($"All Rights Reserved - LFInteractive LLC. (c) 2020-{DateTime.Now.Year}");
            if (args.Contains("--portforward"))
            {
                PortHandler.AddToFirewall();
                log.Warn($"Automatic Port Forwarding is Enabled");
                log.Debug($"Opening Port {port}");
                PortHandler.OpenPort(port);
            }
            InitiateStartupProceedure().ContinueWith(t =>
            {
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
                }).Build().Run();
                log.Info("Shutting Down");
                if (args.Contains("--portforward"))
                {
                    PortHandler.ClosePort(port).Wait();
                    log.Info($"Closed Port: {port}");
                }
            }).Wait();
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