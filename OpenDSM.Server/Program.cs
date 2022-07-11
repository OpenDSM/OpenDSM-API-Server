global using static OpenDSM.Core.Global;
using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using CLPortmapper;
using System.Diagnostics;
using System.Reflection;


namespace OpenDSM.Server;

public class Program
{
    #region Fields

    private static int port = 8080;

    #endregion Fields

    #region Private Methods

    private static void Main(string[] args)
    {
        log.Info($"Welcome to {ApplicationName} Server");
        log.Debug(Copywrite);
        bool useKestrel = true;
        bool portForward = false;
        foreach (string arg in args)
        {
            if (arg.Equals("--iis"))
            {
                useKestrel = false;
            }
            else if (arg.Equals("--firewall"))
            {
                PortHandler.AddToFirewall();
                Environment.Exit(0);
            }
            else if (arg.Equals("--portforward"))
            {
                portForward = true;
            }
        }
        if (portForward)
        {
            PortHandler.AddToFirewall();
            log.Warn($"Automatic Port Forwarding is Enabled");
            log.Debug($"Opening Port {port}");
            PortHandler.OpenPort(port);
        }
        Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            if (useKestrel)
            {
                log.Debug($"Starting server on port {port}");
                builder.UseIISIntegration();
                builder.UseKestrel(options =>
                    {
                        options.ListenAnyIP(port);
                    });
            }
            else
            {
                builder.UseIIS();
            }
            builder.UseStartup<Startup>();
            log.Info("Server is now running!");
        }).Build().Run();

        log.Warn("Shutting Down...");
        if (portForward)
            PortHandler.ClosePort(port);
    }

    #endregion Private Methods
}

internal class Startup
{
    #region Public Methods

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
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