using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SelfDocumentMCP.Models;
using SelfDocumentMCP.Services;
using System.Text;
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
    private readonly JsonSerializerOptions _outputJsonOptions;
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
            WriteIndented = false
        };
        _outputJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        // Debug: Force explicit settings to ensure no indentation
        _outputJsonOptions.WriteIndented = false;
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
            _logger.LogDebug("MCP Server stopped by cancellation request");
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

            _logger.LogTrace("Received request: {Input}", input);

            var request = JsonSerializer.Deserialize<JsonRpcRequest>(input, _jsonOptions);
            if (request == null)
            {
                await SendErrorResponseAsync(null, -32700, "Parse error");
                return;
            }

            var response = await HandleRequestAsync(request);
            if (response != null)
            {
                // Create truly compact JSON manually to ensure no formatting
                var compactJson = CreateCompactJsonResponse(response);

                // Write directly to stdout as UTF-8 bytes
                var jsonBytes = Encoding.UTF8.GetBytes(compactJson);
                await Console.OpenStandardOutput().WriteAsync(jsonBytes, 0, jsonBytes.Length);
                await Console.OpenStandardOutput().WriteAsync(Encoding.UTF8.GetBytes("\n"), 0, 1);
                await Console.OpenStandardOutput().FlushAsync();

                _logger.LogTrace("Sent response: {Response}", compactJson);
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
        _logger.LogDebug("Handling initialize request");

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
        _logger.LogDebug("Handling tools/list request");

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
            },
            new Tool
            {
                Name = "get_recent_commits",
                Description = "Get recent commits from the current repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        count = new { type = "integer", description = "Number of recent commits to retrieve (default: 10)" }
                    }
                }
            },
            new Tool
            {
                Name = "get_changed_files_between_commits",
                Description = "Get list of files changed between two commits",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        commit1 = new { type = "string", description = "First commit hash" },
                        commit2 = new { type = "string", description = "Second commit hash" }
                    },
                    required = new[] { "commit1", "commit2" }
                }
            },
            new Tool
            {
                Name = "get_detailed_diff_between_commits",
                Description = "Get detailed diff content between two commits",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        commit1 = new { type = "string", description = "First commit hash" },
                        commit2 = new { type = "string", description = "Second commit hash" },
                        specificFiles = new { type = "array", items = new { type = "string" }, description = "Optional: specific files to diff" }
                    },
                    required = new[] { "commit1", "commit2" }
                }
            },
            new Tool
            {
                Name = "get_commit_diff_info",
                Description = "Get comprehensive diff information between two commits including file changes and statistics",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        commit1 = new { type = "string", description = "First commit hash" },
                        commit2 = new { type = "string", description = "Second commit hash" }
                    },
                    required = new[] { "commit1", "commit2" }
                }
            },
            new Tool
            {
                Name = "get_file_line_diff_between_commits",
                Description = "Get line-by-line file diff between two commits",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        commit1 = new { type = "string", description = "First commit hash" },
                        commit2 = new { type = "string", description = "Second commit hash" },
                        filePath = new { type = "string", description = "Path to the file to diff" }
                    },
                    required = new[] { "commit1", "commit2", "filePath" }
                }
            },
            new Tool
            {
                Name = "get_local_branches",
                Description = "Get list of local branches in the repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new Tool
            {
                Name = "get_remote_branches",
                Description = "Get list of remote branches in the repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new Tool
            {
                Name = "get_all_branches",
                Description = "Get list of all branches (local and remote) in the repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new Tool
            {
                Name = "fetch_from_remote",
                Description = "Fetch latest changes from remote repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        remoteName = new { type = "string", description = "Name of the remote (default: origin)" }
                    }
                }
            },
            new Tool
            {
                Name = "compare_branches_with_remote",
                Description = "Generate documentation comparing differences between two branches with remote support",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        branch1 = new { type = "string", description = "First branch name (can be local or remote, e.g., 'main' or 'origin/main')" },
                        branch2 = new { type = "string", description = "Second branch name (can be local or remote, e.g., 'feature/xyz' or 'origin/feature/xyz')" },
                        filePath = new { type = "string", description = "Path where to save the documentation file" },
                        outputFormat = new { type = "string", description = "Output format: markdown, html, or text (default: markdown)" },
                        fetchRemote = new { type = "boolean", description = "Whether to fetch from remote before comparison (default: true)" }
                    },
                    required = new[] { "branch1", "branch2", "filePath" }
                }
            },
            new Tool
            {
                Name = "search_commits_for_string",
                Description = "Search all commits for a specific string and return commit details, file names, and line matches",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        searchString = new { type = "string", description = "The string to search for in commit messages and file contents" },
                        maxCommits = new { type = "integer", description = "Maximum number of commits to search through (default: 100)" }
                    },
                    required = new[] { "searchString" }
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
        _logger.LogDebug("Handling tools/call request");

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
            "get_recent_commits" => await HandleGetRecentCommitsAsync(toolRequest),
            "get_changed_files_between_commits" => await HandleGetChangedFilesBetweenCommitsAsync(toolRequest),
            "get_detailed_diff_between_commits" => await HandleGetDetailedDiffBetweenCommitsAsync(toolRequest),
            "get_commit_diff_info" => await HandleGetCommitDiffInfoAsync(toolRequest),
            "get_file_line_diff_between_commits" => await HandleGetFileLineDiffBetweenCommitsAsync(toolRequest),
            "get_local_branches" => await HandleGetLocalBranchesAsync(toolRequest),
            "get_remote_branches" => await HandleGetRemoteBranchesAsync(toolRequest),
            "get_all_branches" => await HandleGetAllBranchesAsync(toolRequest),
            "fetch_from_remote" => await HandleFetchFromRemoteAsync(toolRequest),
            "compare_branches_with_remote" => await HandleCompareBranchesWithRemoteAsync(toolRequest),
            "search_commits_for_string" => await HandleSearchCommitsForStringAsync(toolRequest),
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

    private async Task<CallToolResponse> HandleGetRecentCommitsAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var count = GetArgumentValue<int>(toolRequest.Arguments, "count", 10);

            var commits = await _gitService.GetRecentCommitsAsync(workspaceRoot, count);
            var commitSummary = string.Join("\n", commits.Select(c =>
                $"• {c.Hash[..8]} - {c.Message.Split('\n')[0]} ({c.Author}, {c.Date:yyyy-MM-dd})"));

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = $"Recent {count} commits:\n\n{commitSummary}" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent commits");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetChangedFilesBetweenCommitsAsync(CallToolRequest toolRequest)
    {
        try
        {
            var commit1 = GetArgumentValue<string>(toolRequest.Arguments, "commit1", "");
            var commit2 = GetArgumentValue<string>(toolRequest.Arguments, "commit2", "");

            if (string.IsNullOrEmpty(commit1) || string.IsNullOrEmpty(commit2))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "commit1 and commit2 arguments are required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var changedFiles = await _gitService.GetChangedFilesBetweenCommitsAsync(workspaceRoot, commit1, commit2);
            var filesList = string.Join("\n", changedFiles.Select(f => $"• {f}"));

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = $"Files changed between {commit1[..8]} and {commit2[..8]}:\n\n{filesList}" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting changed files between commits");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetDetailedDiffBetweenCommitsAsync(CallToolRequest toolRequest)
    {
        try
        {
            var commit1 = GetArgumentValue<string>(toolRequest.Arguments, "commit1", "");
            var commit2 = GetArgumentValue<string>(toolRequest.Arguments, "commit2", "");

            if (string.IsNullOrEmpty(commit1) || string.IsNullOrEmpty(commit2))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "commit1 and commit2 arguments are required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var specificFilesArg = GetArgumentValue<object?>(toolRequest.Arguments, "specificFiles", null);
            List<string>? specificFiles = null;

            if (specificFilesArg is JsonElement jsonArray && jsonArray.ValueKind == JsonValueKind.Array)
            {
                specificFiles = jsonArray.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            var detailedDiff = await _gitService.GetDetailedDiffBetweenCommitsAsync(workspaceRoot, commit1, commit2, specificFiles);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = detailedDiff } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed diff between commits");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetCommitDiffInfoAsync(CallToolRequest toolRequest)
    {
        try
        {
            var commit1 = GetArgumentValue<string>(toolRequest.Arguments, "commit1", "");
            var commit2 = GetArgumentValue<string>(toolRequest.Arguments, "commit2", "");

            if (string.IsNullOrEmpty(commit1) || string.IsNullOrEmpty(commit2))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "commit1 and commit2 arguments are required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var diffInfo = await _gitService.GetCommitDiffInfoAsync(workspaceRoot, commit1, commit2);

            var summary = $"Commit Diff Summary ({commit1[..8]} → {commit2[..8]}):\n\n" +
                         $"📊 **Statistics:**\n" +
                         $"• Added files: {diffInfo.AddedFiles.Count}\n" +
                         $"• Modified files: {diffInfo.ModifiedFiles.Count}\n" +
                         $"• Deleted files: {diffInfo.DeletedFiles.Count}\n" +
                         $"• Renamed files: {diffInfo.RenamedFiles.Count}\n" +
                         $"• Total changes: {diffInfo.TotalChanges}\n\n";

            if (diffInfo.AddedFiles.Any())
                summary += $"➕ **Added Files:**\n{string.Join("\n", diffInfo.AddedFiles.Select(f => $"  • {f}"))}\n\n";

            if (diffInfo.ModifiedFiles.Any())
                summary += $"✏️ **Modified Files:**\n{string.Join("\n", diffInfo.ModifiedFiles.Select(f => $"  • {f}"))}\n\n";

            if (diffInfo.DeletedFiles.Any())
                summary += $"🗑️ **Deleted Files:**\n{string.Join("\n", diffInfo.DeletedFiles.Select(f => $"  • {f}"))}\n\n";

            if (diffInfo.RenamedFiles.Any())
                summary += $"📝 **Renamed Files:**\n{string.Join("\n", diffInfo.RenamedFiles.Select(f => $"  • {f}"))}\n\n";

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = summary } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting commit diff info");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetLocalBranchesAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var localBranches = await _gitService.GetLocalBranchesAsync(workspaceRoot);
            var branchList = string.Join("\n", localBranches.Select(b => $"• {b}"));

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = $"Local branches:\n\n{branchList}" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local branches");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetRemoteBranchesAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var remoteBranches = await _gitService.GetRemoteBranchesAsync(workspaceRoot);
            var branchList = string.Join("\n", remoteBranches.Select(b => $"• {b}"));

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = $"Remote branches:\n\n{branchList}" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remote branches");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleGetAllBranchesAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var allBranches = await _gitService.GetAllBranchesAsync(workspaceRoot);
            var branchList = string.Join("\n", allBranches.Select(b => $"• {b}"));

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = $"All branches:\n\n{branchList}" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all branches");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleFetchFromRemoteAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var remoteName = GetArgumentValue<string>(toolRequest.Arguments, "remoteName", "origin");
            var success = await _gitService.FetchFromRemoteAsync(workspaceRoot, remoteName);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent
                {
                    Type = "text",
                    Text = success ? $"Successfully fetched from remote '{remoteName}'" : $"Failed to fetch from remote '{remoteName}'"
                } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from remote");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleCompareBranchesWithRemoteAsync(CallToolRequest toolRequest)
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
            var fetchRemote = GetArgumentValue<bool>(toolRequest.Arguments, "fetchRemote", true);

            // Make path relative to workspace if not absolute
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(workspaceRoot, filePath);
            }

            var commits = await _gitService.GetGitLogsBetweenBranchesWithRemoteAsync(workspaceRoot, branch1, branch2, fetchRemote);
            var documentation = await _gitService.GenerateDocumentationAsync(commits, outputFormat);
            var success = await _gitService.WriteDocumentationToFileAsync(documentation, filePath);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent
                {
                    Type = "text",
                    Text = success ? $"Remote branch comparison documentation successfully written to {filePath}" : "Failed to write documentation to file"
                } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating remote branch comparison documentation");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private async Task<CallToolResponse> HandleSearchCommitsForStringAsync(CallToolRequest toolRequest)
    {
        try
        {
            var searchString = GetArgumentValue<string>(toolRequest.Arguments, "searchString", "");

            if (string.IsNullOrEmpty(searchString))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "searchString argument is required" } }
                };
            }

            var workspaceRoot = Environment.CurrentDirectory;
            var maxCommits = GetArgumentValue<int>(toolRequest.Arguments, "maxCommits", 100);

            var searchResults = await _gitService.SearchCommitsForStringAsync(workspaceRoot, searchString, maxCommits);

            if (!string.IsNullOrEmpty(searchResults.ErrorMessage))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = $"Search error: {searchResults.ErrorMessage}" } }
                };
            }

            // Format the results as a comprehensive text response
            var resultText = FormatSearchResults(searchResults);

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = resultText } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching commits for string");
            return new CallToolResponse
            {
                IsError = true,
                Content = new[] { new ToolContent { Type = "text", Text = $"Error: {ex.Message}" } }
            };
        }
    }

    private string FormatSearchResults(CommitSearchResponse searchResults)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"# Search Results for: '{searchResults.SearchString}'");
        sb.AppendLine();
        sb.AppendLine($"**Summary:**");
        sb.AppendLine($"- Total commits searched: {searchResults.TotalCommitsSearched}");
        sb.AppendLine($"- Commits with matches: {searchResults.TotalMatchingCommits}");
        sb.AppendLine($"- Total line matches: {searchResults.TotalLineMatches}");
        sb.AppendLine();

        if (!searchResults.Results.Any())
        {
            sb.AppendLine("No matches found.");
            return sb.ToString();
        }

        sb.AppendLine("## Matching Commits");
        sb.AppendLine();

        foreach (var result in searchResults.Results.OrderByDescending(r => r.Timestamp))
        {
            sb.AppendLine($"### Commit: {result.CommitHash[..8]}");
            sb.AppendLine($"**Author:** {result.Author}");
            sb.AppendLine($"**Date:** {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**Message:** {result.CommitMessage}");
            sb.AppendLine($"**Total matches in this commit:** {result.TotalMatches}");
            sb.AppendLine();

            foreach (var fileMatch in result.FileMatches)
            {
                sb.AppendLine($"#### File: {fileMatch.FileName}");
                sb.AppendLine($"**Matches in this file:** {fileMatch.LineMatches.Count}");
                sb.AppendLine();

                foreach (var lineMatch in fileMatch.LineMatches)
                {
                    sb.AppendLine($"- **Line {lineMatch.LineNumber}:** `{lineMatch.LineContent}`");
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private async Task<CallToolResponse> HandleGetFileLineDiffBetweenCommitsAsync(CallToolRequest toolRequest)
    {
        try
        {
            var workspaceRoot = Environment.CurrentDirectory;
            var commit1 = GetArgumentValue<string>(toolRequest.Arguments, "commit1", string.Empty);
            var commit2 = GetArgumentValue<string>(toolRequest.Arguments, "commit2", string.Empty);
            var filePath = GetArgumentValue<string>(toolRequest.Arguments, "filePath", string.Empty);

            if (string.IsNullOrEmpty(commit1) || string.IsNullOrEmpty(commit2) || string.IsNullOrEmpty(filePath))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = "Missing required arguments: commit1, commit2, and filePath are required" } }
                };
            }

            var diffInfo = await _gitService.GetFileLineDiffBetweenCommitsAsync(workspaceRoot, commit1, commit2, filePath);

            if (!string.IsNullOrEmpty(diffInfo.ErrorMessage))
            {
                return new CallToolResponse
                {
                    IsError = true,
                    Content = new[] { new ToolContent { Type = "text", Text = diffInfo.ErrorMessage } }
                };
            }

            // Build a formatted result with syntax highlighting
            var sb = new StringBuilder();
            sb.AppendLine($"# Line-by-Line File Diff");
            sb.AppendLine();
            sb.AppendLine($"**File:** `{diffInfo.FilePath}`");
            sb.AppendLine($"**From Commit:** {diffInfo.Commit1}");
            sb.AppendLine($"**To Commit:** {diffInfo.Commit2}");
            sb.AppendLine();
            sb.AppendLine($"**Summary:**");
            sb.AppendLine($"- Added Lines: {diffInfo.AddedLines}");
            sb.AppendLine($"- Deleted Lines: {diffInfo.DeletedLines}");
            sb.AppendLine($"- Modified Lines: {diffInfo.ModifiedLines}");
            sb.AppendLine($"- Total Changes: {diffInfo.AddedLines + diffInfo.DeletedLines + diffInfo.ModifiedLines}");
            sb.AppendLine();

            if (!diffInfo.FileExistsInBothCommits)
            {
                if (diffInfo.AddedLines > 0)
                {
                    sb.AppendLine("*File was added in the second commit*");
                }
                else if (diffInfo.DeletedLines > 0)
                {
                    sb.AppendLine("*File was deleted in the second commit*");
                }
            }

            sb.AppendLine("```diff");
            // Group lines by chunks to improve readability
            var currentChunk = new List<LineDiff>();
            var lastType = "";

            foreach (var line in diffInfo.Lines)
            {
                if (line.Type == "Header" && currentChunk.Any())
                {
                    // Print the current chunk with line numbers
                    foreach (var chunkLine in currentChunk)
                    {
                        var oldNum = chunkLine.OldLineNumber.PadLeft(4);
                        var newNum = chunkLine.NewLineNumber.PadLeft(4);
                        sb.AppendLine($"{oldNum}:{newNum} {chunkLine.Content}");
                    }
                    currentChunk.Clear();
                }

                currentChunk.Add(line);
                lastType = line.Type;
            }

            // Print the last chunk
            if (currentChunk.Any())
            {
                foreach (var chunkLine in currentChunk)
                {
                    var oldNum = chunkLine.OldLineNumber.PadLeft(4);
                    var newNum = chunkLine.NewLineNumber.PadLeft(4);
                    sb.AppendLine($"{oldNum}:{newNum} {chunkLine.Content}");
                }
            }
            sb.AppendLine("```");

            return new CallToolResponse
            {
                Content = new[] { new ToolContent { Type = "text", Text = sb.ToString() } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file line diff between commits");
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
        var compactJson = CreateCompactJsonResponse(response);

        // Write directly to stdout as UTF-8 bytes
        var jsonBytes = Encoding.UTF8.GetBytes(compactJson);
        await Console.OpenStandardOutput().WriteAsync(jsonBytes, 0, jsonBytes.Length);
        await Console.OpenStandardOutput().WriteAsync(Encoding.UTF8.GetBytes("\n"), 0, 1);
        await Console.OpenStandardOutput().FlushAsync();

        await Task.CompletedTask;
    }

    private string CreateCompactJsonResponse(JsonRpcResponse response)
    {
        // Manually build compact JSON to bypass any system formatting
        var result = "{";
        result += $"\"jsonrpc\":\"{response.JsonRpc}\",";
        result += $"\"id\":{(response.Id?.ToString() ?? "null")},";

        if (response.Result != null)
        {
            result += "\"result\":";
            result += JsonSerializer.Serialize(response.Result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }).Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        if (response.Error != null)
        {
            result += "\"error\":";
            result += JsonSerializer.Serialize(response.Error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }).Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        result += "}";
        return result;
    }
}
