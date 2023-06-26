using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Epica.Api.Operacion;

await BuildWebHost(args).RunAsync();
IWebHost BuildWebHost(string[] args) =>
    WebHost
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(cb =>
        {
            var sources = cb.Sources;
            sources.Insert(3, new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource()
            {
                Optional = true,
                Path = "appsettings.localhost.json",
                ReloadOnChange = false
            });
        })
        .UseStartup<Startup>()
        .Build();