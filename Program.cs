using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SelfDocumentMCP.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IMcpServer, McpServer>();
    })
    .UseConsoleLifetime()
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting selfDocumentMCP Server");

try
{
    var mcpServer = host.Services.GetRequiredService<IMcpServer>();
    
    // Handle shutdown gracefully
    var cancellationTokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    };

    await mcpServer.StartAsync(cancellationTokenSource.Token);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while running the MCP server");
    return 1;
}

logger.LogInformation("selfDocumentMCP Server stopped");
return 0;
