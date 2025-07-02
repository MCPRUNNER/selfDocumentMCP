using System.Text.Json.Serialization;

namespace SelfDocumentMCP.Models;

public class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id
    {
        get; set;
    }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params
    {
        get; set;
    }
}

public class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id
    {
        get; set;
    }

    [JsonPropertyName("result")]
    public object? Result
    {
        get; set;
    }

    [JsonPropertyName("error")]
    public JsonRpcError? Error
    {
        get; set;
    }
}

public class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code
    {
        get; set;
    }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data
    {
        get; set;
    }
}

public class InitializeRequest
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public ClientCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("clientInfo")]
    public ClientInfo ClientInfo { get; set; } = new();
}

public class ClientCapabilities
{
    [JsonPropertyName("roots")]
    public RootsCapability? Roots
    {
        get; set;
    }

    [JsonPropertyName("sampling")]
    public object? Sampling
    {
        get; set;
    }
}

public class RootsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged
    {
        get; set;
    }
}

public class ClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

public class InitializeResponse
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public ServerInfo ServerInfo { get; set; } = new();
}

public class ServerCapabilities
{
    [JsonPropertyName("tools")]
    public object? Tools { get; set; } = new { };

    [JsonPropertyName("resources")]
    public object? Resources
    {
        get; set;
    }

    [JsonPropertyName("prompts")]
    public object? Prompts
    {
        get; set;
    }

    [JsonPropertyName("logging")]
    public object? Logging
    {
        get; set;
    }
}

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "selfDocumentMCP";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}

public class Tool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new();
}

public class ToolsListResponse
{
    [JsonPropertyName("tools")]
    public Tool[] Tools { get; set; } = Array.Empty<Tool>();
}

public class CallToolRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments
    {
        get; set;
    }
}

public class CallToolResponse
{
    [JsonPropertyName("content")]
    public ToolContent[] Content { get; set; } = Array.Empty<ToolContent>();

    [JsonPropertyName("isError")]
    public bool IsError
    {
        get; set;
    }
}

public class ToolContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GitCommitInfo
{
    public string Hash { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime Date
    {
        get; set;
    }
    public List<string> ChangedFiles { get; set; } = new();
    public List<string> Changes { get; set; } = new();
}

public class DocumentationRequest
{
    public string? FilePath
    {
        get; set;
    }
    public string? Branch1
    {
        get; set;
    }
    public string? Branch2
    {
        get; set;
    }
    public string? Commit1
    {
        get; set;
    }
    public string? Commit2
    {
        get; set;
    }
    public int? MaxCommits
    {
        get; set;
    }
    public string? OutputFormat
    {
        get; set;
    }
}

public class GitCommitDiffInfo
{
    public string Commit1 { get; set; } = string.Empty;
    public string Commit2 { get; set; } = string.Empty;
    public List<string> AddedFiles { get; set; } = new();
    public List<string> ModifiedFiles { get; set; } = new();
    public List<string> DeletedFiles { get; set; } = new();
    public List<string> RenamedFiles { get; set; } = new();
    public int TotalChanges => AddedFiles.Count + ModifiedFiles.Count + DeletedFiles.Count + RenamedFiles.Count;
    public string DetailedDiff { get; set; } = string.Empty;
}

public class GitFileChange
{
    public string Path { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AddedLines
    {
        get; set;
    }
    public int DeletedLines
    {
        get; set;
    }
}
