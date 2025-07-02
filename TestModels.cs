// Simple test to verify MCP server functionality
// This would normally be a proper unit test project, but this serves as a quick verification

using System.Text.Json;
using SelfDocumentMCP.Models;

public class ModelTests
{
    public static void RunTests()
    {
        Console.WriteLine("selfDocumentMCP Server Test");
        Console.WriteLine("===========================");

        // Test JSON serialization of MCP models
        var initRequest = new InitializeRequest
        {
            ProtocolVersion = "2024-11-05",
            ClientInfo = new ClientInfo
            {
                Name = "test-client",
                Version = "1.0.0"
            }
        };

        var json = JsonSerializer.Serialize(initRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        Console.WriteLine("Serialized InitializeRequest:");
        Console.WriteLine(json);

        // Test JSON-RPC request
        var jsonRpcRequest = new JsonRpcRequest
        {
            Id = 1,
            Method = "initialize",
            Params = initRequest
        };

        var requestJson = JsonSerializer.Serialize(jsonRpcRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        Console.WriteLine("\nSerialized JSON-RPC Request:");
        Console.WriteLine(requestJson);

        Console.WriteLine("\nTest completed successfully!");
        Console.WriteLine("\nTo run the actual MCP server, use: dotnet run");
        Console.WriteLine("Then send JSON-RPC requests via stdin.");
    }
}
