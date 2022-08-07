// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
global using static OpenDSM.Core.Global;

namespace OpenDSM.Server;

public class Program
{
    #region Private Fields

    private static int port = 8080;

    #endregion Private Fields

    #region Private Methods

    private static void Main(string[] args)
    {
        log.Info($"Welcome to {ApplicationName} Server");
        log.Debug(Copywrite);
        bool useKestrel = true;
        foreach (string arg in args)
        {
            if (arg.Equals("--iis"))
            {
                useKestrel = false;
            }
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
            app.UseHttpLogging();
            app.UseMiniProfiler();
        }
        else
        {
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
        }
        app.UseForwardedHeaders();
        app.UseMvc();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseDefaultFiles();
        //app.UseHttpsRedirection();
    }

    public void ConfigureServices(IServiceCollection service)
    {
        service.AddMvc(action =>
        {
            action.EnableEndpointRouting = false;
        });
        service.AddMiniProfiler();
    }

    #endregion Public Methods
}