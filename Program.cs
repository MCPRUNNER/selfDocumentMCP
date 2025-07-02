using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SelfDocumentMCP.Services;
using System.Text;

// Ensure UTF-8 encoding for stdout
Console.OutputEncoding = Encoding.UTF8;

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
            builder.ClearProviders();

            // Add file logging instead of console logging
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "selfdocumentmcp.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            builder.AddFile(logPath, minimumLevel: LogLevel.Information);

            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IMcpServer, McpServer>();
    })
    .UseConsoleLifetime(options =>
    {
        options.SuppressStatusMessages = true;
    })
    .Build();

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
    // Log to stderr
    await Console.Error.WriteLineAsync($"Error: {ex.Message}");
    return 1;
}

return 0;
