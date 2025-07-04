using System;
using System.Collections.Generic;
using System.Text.Json;
using GitVisionMCP.Models;
using Xunit;

namespace GitVisionMCP.Tests.Models
{
    public class McpModelsTests
    {
        [Fact]
        public void JsonRpcRequest_Serialization_Works()
        {
            var req = new JsonRpcRequest
            {
                Id = 1,
                Method = "testMethod",
                Params = new { foo = "bar" }
            };
            var json = JsonSerializer.Serialize(req);
            Assert.Contains("\"jsonrpc\":\"2.0\"", json);
            Assert.Contains("\"id\":1", json);
            Assert.Contains("\"method\":\"testMethod\"", json);
            Assert.Contains("\"params\":{", json);
        }

        [Fact]
        public void JsonRpcResponse_Serialization_Works()
        {
            var resp = new JsonRpcResponse
            {
                Id = 2,
                Result = new { value = 42 },
                Error = null
            };
            var json = JsonSerializer.Serialize(resp);
            Assert.Contains("\"jsonrpc\":\"2.0\"", json);
            Assert.Contains("\"id\":2", json);
            Assert.Contains("\"result\":{", json);
        }

        [Fact]
        public void JsonRpcError_Properties_Work()
        {
            var err = new JsonRpcError
            {
                Code = -32601,
                Message = "Method not found",
                Data = "Extra info"
            };
            Assert.Equal(-32601, err.Code);
            Assert.Equal("Method not found", err.Message);
            Assert.Equal("Extra info", err.Data);
        }

        [Fact]
        public void InitializeRequest_Defaults_AreSet()
        {
            var req = new InitializeRequest();
            Assert.Equal("2024-11-05", req.ProtocolVersion);
            Assert.NotNull(req.Capabilities);
            Assert.NotNull(req.ClientInfo);
        }

        [Fact]
        public void InitializeResponse_Defaults_AreSet()
        {
            var resp = new InitializeResponse();
            Assert.Equal("2024-11-05", resp.ProtocolVersion);
            Assert.NotNull(resp.Capabilities);
            Assert.NotNull(resp.ServerInfo);
        }

        [Fact]
        public void ServerInfo_Defaults_AreSet()
        {
            var info = new ServerInfo();
            Assert.Equal("GitVisionMCP", info.Name);
            Assert.Equal("1.0.0", info.Version);
        }

        [Fact]
        public void Tool_Properties_Work()
        {
            var tool = new Tool
            {
                Name = "test",
                Description = "desc",
                InputSchema = new { type = "object" }
            };
            Assert.Equal("test", tool.Name);
            Assert.Equal("desc", tool.Description);
            Assert.NotNull(tool.InputSchema);
        }

        [Fact]
        public void GitCommitInfo_Properties_Work()
        {
            var commit = new GitCommitInfo
            {
                Hash = "abc123",
                Message = "msg",
                Author = "author",
                AuthorEmail = "a@b.com",
                Date = DateTime.UtcNow,
                ChangedFiles = new List<string> { "file1" },
                Changes = new List<string> { "diff" }
            };
            Assert.Equal("abc123", commit.Hash);
            Assert.Equal("msg", commit.Message);
            Assert.Equal("author", commit.Author);
            Assert.Equal("a@b.com", commit.AuthorEmail);
            Assert.Single(commit.ChangedFiles);
            Assert.Single(commit.Changes);
        }

        [Fact]
        public void GitCommitDiffInfo_TotalChanges_CalculatesCorrectly()
        {
            var diff = new GitCommitDiffInfo
            {
                AddedFiles = new List<string> { "a" },
                ModifiedFiles = new List<string> { "b" },
                DeletedFiles = new List<string> { "c" },
                RenamedFiles = new List<string> { "d" }
            };
            Assert.Equal(4, diff.TotalChanges);
        }

        [Fact]
        public void CommitSearchResponse_TotalLineMatches_CalculatesCorrectly()
        {
            var resp = new CommitSearchResponse
            {
                Results = new List<CommitSearchResult>
                {
                    new CommitSearchResult
                    {
                        FileMatches = new List<FileSearchMatch>
                        {
                            new FileSearchMatch { LineMatches = new List<LineSearchMatch> { new LineSearchMatch(), new LineSearchMatch() } },
                            new FileSearchMatch { LineMatches = new List<LineSearchMatch> { new LineSearchMatch() } }
                        }
                    },
                    new CommitSearchResult
                    {
                        FileMatches = new List<FileSearchMatch>
                        {
                            new FileSearchMatch { LineMatches = new List<LineSearchMatch> { new LineSearchMatch() } }
                        }
                    }
                }
            };
            Assert.Equal(4, resp.TotalLineMatches);
        }
    }
}
