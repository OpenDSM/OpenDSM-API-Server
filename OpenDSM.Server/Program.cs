// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
global using static OpenDSM.Core.Global;
using System.Net;
using HashidsNet;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenDSM.Core.Handlers;
namespace OpenDSM.Server;

public class Program
{

    #region Private Fields

    private static int port = 8080;

    #endregion Private Fields

    #region Private Methods

    private static void Main()
    {
        log.Info($"Welcome to {ApplicationName} Server");
        log.Debug(Copywrite);

        InitHandlers();

        Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            log.Debug($"Starting server on port {port}");
            builder.UseKestrel(options =>
                {
                    options.ListenAnyIP(port);
                });
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

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseHttpsRedirection();
        }
        wwwroot = env.WebRootPath;
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().Build());
        app.UseMvc();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseDefaultFiles();
    }

    public void ConfigureServices(IServiceCollection service)
    {
        service.Configure<ForwardedHeadersOptions>(options =>
        {
            options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
        });
        service.AddMvc(action =>
        {
            action.EnableEndpointRouting = false;
        });
        //   service.AddMiniProfiler();


    }

    #endregion Public Methods

}