using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SelfDocumentMCP.Models;
using SelfDocumentMCP.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SelfDocumentMCP.Services;

public interface IMcpServer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public class McpServer : IMcpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGitService _gitService;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _isRunning;

    public McpServer(ILogger<McpServer> logger, IConfiguration configuration, IGitService gitService)
    {
        _logger = logger;
        _configuration = configuration;
        _gitService = gitService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting MCP Server...");
        _isRunning = true;

        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                await ProcessRequestAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MCP Server stopped by cancellation request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP Server main loop");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping MCP Server...");
        _isRunning = false;
        await Task.CompletedTask;
    }

    private async Task ProcessRequestAsync(CancellationToken cancellationToken)
    {
        try
        {
            var input = await Console.In.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            _logger.LogDebug("Received request: {Input}", input);

            var request = JsonSerializer.Deserialize<JsonRpcRequest>(input, _jsonOptions);
            if (request == null)
            {
                await SendErrorResponseAsync(null, -32700, "Parse error");
                return;
            }

            var response = await HandleRequestAsync(request);
            if (response != null)
            {
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                Console.WriteLine(responseJson);
                _logger.LogDebug("Sent response: {Response}", responseJson);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error");
            await SendErrorResponseAsync(null, -32700, "Parse error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            await SendErrorResponseAsync(null, -32603, "Internal error");
        }
    }

    private async Task<JsonRpcResponse?> HandleRequestAsync(JsonRpcRequest request)
    {
        try
        {
            return request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request),
                "initialized" => null, // Notification, no response needed
                "tools/list" => await HandleToolsListAsync(request),
                "tools/call" => await HandleToolCallAsync(request),
                _ => CreateErrorResponse(request.Id, -32601, "Method not found")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request method: {Method}", request.Method);
            return CreateErrorResponse(request.Id, -32603, "Internal error", ex.Message);
        }
    }

    private async Task<JsonRpcResponse> HandleInitializeAsync(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling initialize request");

        var initResponse = new InitializeResponse
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new ServerCapabilities
            {
                Tools = new { },
                Resources = null,
                Prompts = null,
                Logging = new { }
            },
            ServerInfo = new ServerInfo
            {
                Name = "selfDocumentMCP",
                Version = "1.0.0"
            }
        };

        return await Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = initResponse
        });
    }

    private async Task<JsonRpcResponse> HandleToolsListAsync(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling tools/list request");

        var tools = new[]
        {
            new Tool
            {
                Name = "generate_git_documentation",
                Description = "Generate documentation from git logs for the current workspace",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        maxCommits = new { type = "integer", description = "Maximum number of commits to include (default: 50)" },
                        outputFormat = new { type = "string", description = "Output format: markdown, html, or text (default: markdown)" }
                    }
                }
            },
            new Tool
            {
                Name = "generate_git_documentation_to_file",
                Description = "Generate documentation from git logs and write to a file",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        filePath = new { type = "string", description = "Path where to save the documentation file" },
                        maxCommits = new { type = "integer", description = "Maximum number of commits to include (default: 50)" },
                        outputFormat = new { type = "string", description = "Output format: markdown, html, or text (default: markdown)" }
                    },
                    required = new[] { "filePath" }
                }
            },
            new Tool
            {
                Name = "compare_branches_documentation",
                Description = "Generate documentation comparing differences between two branches",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        branch1 = new { type = "string", description = "First branch name" },
                        branch2 = new { type = "string", description = "Second branch name" },
                        filePath = new { type = "string", description = "Path where to save the documentation file" },
                        outputFormat = new { type = "string", description = "Output format: markdown, html, or text (default: markdown)" }
                    },
                    required = new[] { "branch1", "branch2", "filePath" }
                }
            },
            new Tool
            {
                Name = "compare_commits_documentation",
                Description = "Generate documentation comparing differences between two commits",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        commit1 = new { type = "string", description = "First commit hash" },
                        commit2 = new { type = "string", description = "Second commit hash" },
                        filePath = new { type = "string", description = "Path where to save the documentation file" },
                        outputFormat = new { type = "string", description = "Output format: markdown, html, or text (default: markdown)" }
                    },
                    required = new[] { "commit1", "commit2", "filePath" }
                }
            }
        };

        var response = new ToolsListResponse { Tools = tools };

        return await Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = response
        });
    }

    private async Task<JsonRpcResponse> HandleToolCallAsync(JsonRpcRequest request)
    {
        _logger.LogInformation("Handling tools/call request");

        if (request.Params == null)
        {
            return CreateErrorResponse(request.Id, -32602, "Invalid params");
        }

        var toolRequest = JsonSerializer.Deserialize<CallToolRequest>(
            JsonSerializer.Serialize(request.Params), _jsonOptions);

        if (toolRequest == null)
        {
            return CreateErrorResponse(request.Id, -32602, "Invalid tool request");
        }

        var response = toolRequest.Name switch
        {
            "generate_git_documentation" => await HandleGenerateGitDocumentationAsync(toolRequest),
            "generate_git_documentation_to_file" => await HandleGenerateGitDocumentationToFileAsync(toolRequest),
            "compare_branches_documentation" => await HandleCompareBranchesDocumentationAsync(toolRequest),
            "compare_commits_documentation" => await HandleCompareCommitsDocumentationAsync(toolRequest),
            _ => new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = "Unknown tool" } }
            }
        };

        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = response
        };
    }

    private async Task<CallToolResponse> HandleGenerateGitDocumentationAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var maxCommits = GetArgumentValue<int>(toolRequest.Arguments, "maxCommits", 50);
            var outputFormat = GetArgumentValue<string>(toolRequest.Arguments, "outputFormat", "markdown");

            var commits = await _gitService.GetGitLogsAsync(workspaceRoot, maxCommits);
            var documentation = await _gitService.GenerateDocumentationAsync(commits, outputFormat);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = documentation } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating git documentation");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGenerateGitDocumentationToFileAsync(CallToolRequest toolRequest)
    {
        try
        {
            var filePath = GetArgumentValue<string>(toolRequest.Arguments, "filePath", "");
            if (string.IsNullOrEmpty(filePath))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "filePath argument is required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var maxCommits = GetArgumentValue<int>(toolRequest.Arguments, "maxCommits", 50);
            var outputFormat = GetArgumentValue<string>(toolRequest.Arguments, "outputFormat", "markdown");

            // Make path relative to workspace if not absolute
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(workspaceRoot, filePath);
            }

            var commits = await _gitService.GetGitLogsAsync(workspaceRoot, maxCommits);
            var documentation = await _gitService.GenerateDocumentationAsync(commits, outputFormat);
            var success = await _gitService.WriteDocumentationToFileAsync(documentation, filePath);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent 
                { 
                    Type = "text", 
                    Text = success ? $"Documentation successfully written to {filePath}" : "Failed to write documentation to file" 
                } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating git documentation to file");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleCompareBranchesDocumentationAsync(CallToolRequest toolRequest)
    {
        try
        {
            var branch1 = GetArgumentValue<string>(toolRequest.Arguments, "branch1", "");
            var branch2 = GetArgumentValue<string>(toolRequest.Arguments, "branch2", "");
            var filePath = GetArgumentValue<string>(toolRequest.Arguments, "filePath", "");

            if (string.IsNullOrEmpty(branch1) || string.IsNullOrEmpty(branch2) || string.IsNullOrEmpty(filePath))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "branch1, branch2, and filePath arguments are required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var outputFormat = GetArgumentValue<string>(toolRequest.Arguments, "outputFormat", "markdown");

            // Make path relative to workspace if not absolute
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(workspaceRoot, filePath);
            }

            var commits = await _gitService.GetGitLogsBetweenBranchesAsync(workspaceRoot, branch1, branch2);
            var documentation = await _gitService.GenerateDocumentationAsync(commits, outputFormat);
            var success = await _gitService.WriteDocumentationToFileAsync(documentation, filePath);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent 
                { 
                    Type = "text", 
                    Text = success ? $"Branch comparison documentation successfully written to {filePath}" : "Failed to write documentation to file" 
                } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating branch comparison documentation");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleCompareCommitsDocumentationAsync(CallToolRequest toolRequest)
    {
        try
        {
            var commit1 = GetArgumentValue<string>(toolRequest.Arguments, "commit1", "");
            var commit2 = GetArgumentValue<string>(toolRequest.Arguments, "commit2", "");
            var filePath = GetArgumentValue<string>(toolRequest.Arguments, "filePath", "");

            if (string.IsNullOrEmpty(commit1) || string.IsNullOrEmpty(commit2) || string.IsNullOrEmpty(filePath))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "commit1, commit2, and filePath arguments are required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var outputFormat = GetArgumentValue<string>(toolRequest.Arguments, "outputFormat", "markdown");

            // Make path relative to workspace if not absolute
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(workspaceRoot, filePath);
            }

            var commits = await _gitService.GetGitLogsBetweenCommitsAsync(workspaceRoot, commit1, commit2);
            var documentation = await _gitService.GenerateDocumentationAsync(commits, outputFormat);
            var success = await _gitService.WriteDocumentationToFileAsync(documentation, filePath);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent 
                { 
                    Type = "text", 
                    Text = success ? $"Commit comparison documentation successfully written to {filePath}" : "Failed to write documentation to file" 
                } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating commit comparison documentation");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private T GetArgumentValue<T>(Dictionary<string, object>? arguments, string key, T defaultValue)
    {
        if (arguments == null || !arguments.ContainsKey(key))
            return defaultValue;

        try
        {
            var value = arguments[key];
            if (value is JsonElement jsonElement)
            {
                return jsonElement.Deserialize<T>(_jsonOptions) ?? defaultValue;
            }
            return (T)Convert.ChangeType(value, typeof(T)) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private JsonRpcResponse CreateErrorResponse(object? id, int code, string message, object? data = null)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
    }

    private async Task SendErrorResponseAsync(object? id, int code, string message)
    {
        var response = CreateErrorResponse(id, code, message);
        var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
        Console.WriteLine(responseJson);
        await Task.CompletedTask;
    }
}
