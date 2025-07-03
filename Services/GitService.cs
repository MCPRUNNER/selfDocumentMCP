using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using SelfDocumentMCP.Models;

namespace SelfDocumentMCP.Services;

public interface IGitService
{
    Task<List<GitCommitInfo>> GetGitLogsAsync(string repositoryPath, int maxCommits = 50);
    Task<List<GitCommitInfo>> GetGitLogsBetweenBranchesAsync(string repositoryPath, string branch1, string branch2);
    Task<List<GitCommitInfo>> GetGitLogsBetweenCommitsAsync(string repositoryPath, string commit1, string commit2);
    Task<string> GenerateDocumentationAsync(List<GitCommitInfo> commits, string format = "markdown");
    Task<bool> WriteDocumentationToFileAsync(string content, string filePath);

    // New methods for enhanced git operations
    Task<List<GitCommitInfo>> GetRecentCommitsAsync(string repositoryPath, int count = 10);
    Task<List<string>> GetChangedFilesBetweenCommitsAsync(string repositoryPath, string commit1, string commit2);
    Task<string> GetDetailedDiffBetweenCommitsAsync(string repositoryPath, string commit1, string commit2, List<string>? specificFiles = null);
    Task<GitCommitDiffInfo> GetCommitDiffInfoAsync(string repositoryPath, string commit1, string commit2);
    Task<FileLineDiffInfo> GetFileLineDiffBetweenCommitsAsync(string repositoryPath, string commit1, string commit2, string filePath);

    // New methods for remote branch support
    Task<List<string>> GetLocalBranchesAsync(string repositoryPath);
    Task<List<string>> GetRemoteBranchesAsync(string repositoryPath);
    Task<List<string>> GetAllBranchesAsync(string repositoryPath);
    Task<bool> FetchFromRemoteAsync(string repositoryPath, string remoteName = "origin");
    Task<List<GitCommitInfo>> GetGitLogsBetweenBranchesWithRemoteAsync(string repositoryPath, string branch1, string branch2, bool fetchRemote = true);
}

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task<List<GitCommitInfo>> GetGitLogsAsync(string repositoryPath, int maxCommits = 50)
    {
        try
        {
            // Validate repository path
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentException("Repository path cannot be null or empty", nameof(repositoryPath));
            }

            if (!Directory.Exists(repositoryPath))
            {
                throw new DirectoryNotFoundException($"Repository path does not exist: {repositoryPath}");
            }

            // Check if it's a git repository
            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            _logger.LogInformation("Getting git logs from repository: {RepositoryPath}", repositoryPath);

            var commits = new List<GitCommitInfo>();

            using var repo = new Repository(repositoryPath);

            // Check if repository has any commits
            if (!repo.Commits.Any())
            {
                _logger.LogWarning("Repository has no commits");
                return commits;
            }

            var repoCommits = repo.Commits.Take(Math.Max(1, maxCommits));

            foreach (var commit in repoCommits)
            {
                var commitInfo = await CreateGitCommitInfoAsync(repo, commit);
                commits.Add(commitInfo);
            }

            _logger.LogInformation("Retrieved {Count} commits", commits.Count);
            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git logs from {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    public async Task<List<GitCommitInfo>> GetGitLogsBetweenBranchesAsync(string repositoryPath, string branch1, string branch2)
    {
        try
        {
            _logger.LogInformation("Getting git logs between branches {Branch1} and {Branch2}", branch1, branch2);

            var commits = new List<GitCommitInfo>();

            using var repo = new Repository(repositoryPath);

            // Handle both local and remote branch references
            var branch1Name = NormalizeBranchName(branch1);
            var branch2Name = NormalizeBranchName(branch2);

            var branch1Ref = repo.Branches[branch1Name] ?? repo.Branches[$"origin/{branch1Name}"] ?? repo.Branches[branch1];
            var branch2Ref = repo.Branches[branch2Name] ?? repo.Branches[$"origin/{branch2Name}"] ?? repo.Branches[branch2];

            if (branch1Ref == null)
            {
                throw new ArgumentException($"Branch '{branch1}' not found (tried local and remote variants)");
            }

            if (branch2Ref == null)
            {
                throw new ArgumentException($"Branch '{branch2}' not found (tried local and remote variants)");
            }

            var filter = new CommitFilter
            {
                ExcludeReachableFrom = branch1Ref.Tip,
                IncludeReachableFrom = branch2Ref.Tip
            };

            var repoCommits = repo.Commits.QueryBy(filter);

            foreach (var commit in repoCommits)
            {
                var commitInfo = await CreateGitCommitInfoAsync(repo, commit);
                commits.Add(commitInfo);
            }

            _logger.LogInformation("Retrieved {Count} commits between branches", commits.Count);
            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git logs between branches {Branch1} and {Branch2}", branch1, branch2);
            throw;
        }
    }

    public async Task<List<GitCommitInfo>> GetGitLogsBetweenCommitsAsync(string repositoryPath, string commit1, string commit2)
    {
        try
        {
            _logger.LogInformation("Getting git logs between commits {Commit1} and {Commit2}", commit1, commit2);

            var commits = new List<GitCommitInfo>();

            using var repo = new Repository(repositoryPath);
            var commit1Obj = repo.Lookup<Commit>(commit1);
            var commit2Obj = repo.Lookup<Commit>(commit2);

            if (commit1Obj == null || commit2Obj == null)
            {
                throw new ArgumentException("One or both commits not found");
            }

            var filter = new CommitFilter
            {
                ExcludeReachableFrom = commit1Obj,
                IncludeReachableFrom = commit2Obj
            };

            var repoCommits = repo.Commits.QueryBy(filter);

            foreach (var commit in repoCommits)
            {
                var commitInfo = await CreateGitCommitInfoAsync(repo, commit);
                commits.Add(commitInfo);
            }

            _logger.LogInformation("Retrieved {Count} commits between commits", commits.Count);
            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git logs between commits {Commit1} and {Commit2}", commit1, commit2);
            throw;
        }
    }

    public async Task<string> GenerateDocumentationAsync(List<GitCommitInfo> commits, string format = "markdown")
    {
        try
        {
            _logger.LogInformation("Generating documentation for {Count} commits in {Format} format", commits.Count, format);

            var documentation = format.ToLower() switch
            {
                "markdown" => await GenerateMarkdownDocumentationAsync(commits),
                "html" => await GenerateHtmlDocumentationAsync(commits),
                "text" => await GenerateTextDocumentationAsync(commits),
                _ => await GenerateMarkdownDocumentationAsync(commits)
            };

            _logger.LogInformation("Generated documentation with {Length} characters", documentation.Length);
            return documentation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation");
            throw;
        }
    }

    public async Task<bool> WriteDocumentationToFileAsync(string content, string filePath)
    {
        try
        {
            _logger.LogInformation("Writing documentation to file: {FilePath}", filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content);

            _logger.LogInformation("Successfully wrote documentation to {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing documentation to file {FilePath}", filePath);
            return false;
        }
    }

    public async Task<List<GitCommitInfo>> GetRecentCommitsAsync(string repositoryPath, int count = 10)
    {
        try
        {
            _logger.LogInformation("Getting {Count} recent commits from repository: {RepositoryPath}", count, repositoryPath);

            var commits = new List<GitCommitInfo>();

            using var repo = new Repository(repositoryPath);

            // Get the most recent commits
            var repoCommits = repo.Commits.Take(count);

            foreach (var commit in repoCommits)
            {
                var commitInfo = await CreateGitCommitInfoAsync(repo, commit);
                commits.Add(commitInfo);
            }

            _logger.LogInformation("Retrieved {Count} recent commits", commits.Count);
            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent commits from {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    public async Task<List<string>> GetChangedFilesBetweenCommitsAsync(string repositoryPath, string commit1, string commit2)
    {
        try
        {
            _logger.LogInformation("Getting changed files between commits {Commit1} and {Commit2}", commit1, commit2);

            var changedFiles = new List<string>();

            using var repo = new Repository(repositoryPath);
            var commit1Obj = repo.Lookup<Commit>(commit1);
            var commit2Obj = repo.Lookup<Commit>(commit2);

            if (commit1Obj == null || commit2Obj == null)
            {
                throw new ArgumentException("One or both commits not found");
            }

            // Get the diff between the two commits
            var changes = repo.Diff.Compare<TreeChanges>(commit1Obj.Tree, commit2Obj.Tree);

            foreach (var change in changes)
            {
                changedFiles.Add(change.Path);
            }

            _logger.LogInformation("Retrieved {Count} changed files between commits", changedFiles.Count);
            return await Task.FromResult(changedFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting changed files between commits {Commit1} and {Commit2}", commit1, commit2);
            throw;
        }
    }

    public async Task<string> GetDetailedDiffBetweenCommitsAsync(string repositoryPath, string commit1, string commit2, List<string>? specificFiles = null)
    {
        try
        {
            _logger.LogInformation("Getting detailed diff between commits {Commit1} and {Commit2}", commit1, commit2);

            var diffBuilder = new System.Text.StringBuilder();

            using var repo = new Repository(repositoryPath);
            var commit1Obj = repo.Lookup<Commit>(commit1);
            var commit2Obj = repo.Lookup<Commit>(commit2);

            if (commit1Obj == null || commit2Obj == null)
            {
                throw new ArgumentException("One or both commits not found");
            }

            // Get the diff between the two commits
            var changes = repo.Diff.Compare<TreeChanges>(commit1Obj.Tree, commit2Obj.Tree);

            foreach (var change in changes)
            {
                // If specific files are provided, only include changes for those files
                if (specificFiles != null && !specificFiles.Contains(change.Path))
                {
                    continue;
                }

                diffBuilder.AppendLine($"diff --git a/{change.OldPath} b/{change.Path}");
                diffBuilder.AppendLine($"--- a/{change.OldPath}");
                diffBuilder.AppendLine($"+++ b/{change.Path}");
                diffBuilder.AppendLine($"Status: {change.Status}");
                diffBuilder.AppendLine();
            }

            _logger.LogInformation("Retrieved diff for {Count} files", changes.Count());
            return await Task.FromResult(diffBuilder.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting detailed diff between commits {Commit1} and {Commit2}", commit1, commit2);
            throw;
        }
    }

    public async Task<GitCommitDiffInfo> GetCommitDiffInfoAsync(string repositoryPath, string commit1, string commit2)
    {
        try
        {
            _logger.LogInformation("Getting commit diff info between {Commit1} and {Commit2}", commit1, commit2);

            var diffInfo = new GitCommitDiffInfo();

            using var repo = new Repository(repositoryPath);
            var commit1Obj = repo.Lookup<Commit>(commit1);
            var commit2Obj = repo.Lookup<Commit>(commit2);

            if (commit1Obj == null || commit2Obj == null)
            {
                throw new ArgumentException("One or both commits not found");
            }

            // Get the diff between the two commits
            var changes = repo.Diff.Compare<TreeChanges>(commit1Obj.Tree, commit2Obj.Tree);

            diffInfo.Commit1 = commit1;
            diffInfo.Commit2 = commit2;

            foreach (var change in changes)
            {
                switch (change.Status)
                {
                    case ChangeKind.Added:
                        diffInfo.AddedFiles.Add(change.Path);
                        break;
                    case ChangeKind.Modified:
                        diffInfo.ModifiedFiles.Add(change.Path);
                        break;
                    case ChangeKind.Deleted:
                        diffInfo.DeletedFiles.Add(change.Path);
                        break;
                    case ChangeKind.Renamed:
                        diffInfo.RenamedFiles.Add($"{change.OldPath} -> {change.Path}");
                        break;
                }
            }

            _logger.LogInformation("Retrieved diff info for {Count} files", diffInfo.TotalChanges);
            return await Task.FromResult(diffInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting commit diff info between {Commit1} and {Commit2}", commit1, commit2);
            throw;
        }
    }

    public Task<FileLineDiffInfo> GetFileLineDiffBetweenCommitsAsync(string repositoryPath, string commit1, string commit2, string filePath)
    {
        _logger.LogInformation("Getting line-by-line file diff between commits {Commit1} and {Commit2} for file {FilePath}", commit1, commit2, filePath);

        var result = new FileLineDiffInfo
        {
            FilePath = filePath,
            Commit1 = commit1,
            Commit2 = commit2
        };

        try
        {
            if (!Repository.IsValid(repositoryPath))
            {
                result.ErrorMessage = $"Path is not a valid git repository: {repositoryPath}";
                return Task.FromResult(result);
            }

            using var repo = new Repository(repositoryPath);

            // Get commit objects
            var commitObj1 = repo.Lookup<Commit>(commit1);
            if (commitObj1 == null)
            {
                result.ErrorMessage = $"Invalid commit hash: {commit1}";
                return Task.FromResult(result);
            }

            var commitObj2 = repo.Lookup<Commit>(commit2);
            if (commitObj2 == null)
            {
                result.ErrorMessage = $"Invalid commit hash: {commit2}";
                return Task.FromResult(result);
            }

            // Get the file tree entries from both commits
            var tree1 = commitObj1.Tree;
            var tree2 = commitObj2.Tree;

            // Check if the file exists in both commits
            var fileEntry1 = tree1[filePath];
            var fileEntry2 = tree2[filePath];

            if (fileEntry1 == null && fileEntry2 == null)
            {
                result.ErrorMessage = $"File {filePath} does not exist in either commit";
                return Task.FromResult(result);
            }

            result.FileExistsInBothCommits = fileEntry1 != null && fileEntry2 != null;

            // Prepare for diff
            var diffOptions = new CompareOptions
            {
                ContextLines = 3,
                InterhunkLines = 1,
                IncludeUnmodified = true
            };

            var patchText = string.Empty;

            // Get the diff between the two file versions
            if (fileEntry1 != null && fileEntry2 != null)
            {
                // File exists in both commits - compare content
                var patch = repo.Diff.Compare<Patch>(tree1, tree2, new[] { filePath }, diffOptions);
                patchText = patch?.Content ?? string.Empty;
            }
            else if (fileEntry1 != null)
            {
                // File only exists in the first commit - show as deletion
                var patch = repo.Diff.Compare<Patch>(tree1, null, new[] { filePath }, diffOptions);
                patchText = patch?.Content ?? string.Empty;
                if (fileEntry1.TargetType == TreeEntryTargetType.Blob)
                {
                    result.DeletedLines = result.TotalLines = CountLines(fileEntry1);
                }
            }
            else if (fileEntry2 != null)
            {
                // File only exists in the second commit - show as addition
                var patch = repo.Diff.Compare<Patch>(null, tree2, new[] { filePath }, diffOptions);
                patchText = patch?.Content ?? string.Empty;
                if (fileEntry2.TargetType == TreeEntryTargetType.Blob)
                {
                    result.AddedLines = result.TotalLines = CountLines(fileEntry2);
                }
            }

            // Parse the unified diff format
            var lines = ParseUnifiedDiff(patchText);
            result.Lines = lines;

            // Calculate stats
            result.AddedLines = lines.Count(l => l.Type == "Added");
            result.DeletedLines = lines.Count(l => l.Type == "Deleted");
            result.ModifiedLines = lines.Count(l => l.Type == "Modified");
            result.TotalLines = lines.Count;

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file line diff between commits {Commit1} and {Commit2} for file {FilePath}", commit1, commit2, filePath);
            result.ErrorMessage = $"Error: {ex.Message}";
            return Task.FromResult(result);
        }
    }

    private async Task<GitCommitInfo> CreateGitCommitInfoAsync(Repository repo, Commit commit)
    {
        var commitInfo = new GitCommitInfo
        {
            Hash = commit.Sha,
            Message = commit.Message,
            Author = commit.Author.Name,
            AuthorEmail = commit.Author.Email,
            Date = commit.Author.When.DateTime
        };

        // Get changed files and changes
        if (commit.Parents.Any())
        {
            var parent = commit.Parents.First();
            var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);

            foreach (var change in changes)
            {
                commitInfo.ChangedFiles.Add(change.Path);
                commitInfo.Changes.Add($"{change.Status}: {change.Path}");
            }
        }

        return await Task.FromResult(commitInfo);
    }

    private async Task<string> GenerateMarkdownDocumentationAsync(List<GitCommitInfo> commits)
    {
        var markdown = new System.Text.StringBuilder();

        markdown.AppendLine("# Git Commit Documentation");
        markdown.AppendLine();
        markdown.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdown.AppendLine($"Total commits: {commits.Count}");
        markdown.AppendLine();

        foreach (var commit in commits.OrderByDescending(c => c.Date))
        {
            markdown.AppendLine($"## Commit: {commit.Hash[..8]}");
            markdown.AppendLine();
            markdown.AppendLine($"**Author:** {commit.Author} <{commit.AuthorEmail}>");
            markdown.AppendLine($"**Date:** {commit.Date:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine();
            markdown.AppendLine($"**Message:**");
            markdown.AppendLine($"```");
            markdown.AppendLine(commit.Message);
            markdown.AppendLine($"```");
            markdown.AppendLine();

            if (commit.ChangedFiles.Any())
            {
                markdown.AppendLine($"**Changed Files:**");
                foreach (var file in commit.ChangedFiles)
                {
                    markdown.AppendLine($"- {file}");
                }
                markdown.AppendLine();
            }

            if (commit.Changes.Any())
            {
                markdown.AppendLine($"**Changes:**");
                foreach (var change in commit.Changes)
                {
                    markdown.AppendLine($"- {change}");
                }
                markdown.AppendLine();
            }

            markdown.AppendLine("---");
            markdown.AppendLine();
        }

        return await Task.FromResult(markdown.ToString());
    }

    private async Task<string> GenerateHtmlDocumentationAsync(List<GitCommitInfo> commits)
    {
        var html = new System.Text.StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<title>Git Commit Documentation</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("h1 { color: #333; }");
        html.AppendLine("h2 { color: #666; border-bottom: 1px solid #ccc; }");
        html.AppendLine("pre { background-color: #f4f4f4; padding: 10px; border-radius: 4px; }");
        html.AppendLine("ul { margin: 10px 0; }");
        html.AppendLine("hr { margin: 20px 0; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("<h1>Git Commit Documentation</h1>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine($"<p>Total commits: {commits.Count}</p>");

        foreach (var commit in commits.OrderByDescending(c => c.Date))
        {
            html.AppendLine($"<h2>Commit: {commit.Hash[..8]}</h2>");
            html.AppendLine($"<p><strong>Author:</strong> {commit.Author} &lt;{commit.AuthorEmail}&gt;</p>");
            html.AppendLine($"<p><strong>Date:</strong> {commit.Date:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p><strong>Message:</strong></p>");
            html.AppendLine($"<pre>{System.Net.WebUtility.HtmlEncode(commit.Message)}</pre>");

            if (commit.ChangedFiles.Any())
            {
                html.AppendLine($"<p><strong>Changed Files:</strong></p>");
                html.AppendLine("<ul>");
                foreach (var file in commit.ChangedFiles)
                {
                    html.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(file)}</li>");
                }
                html.AppendLine("</ul>");
            }

            if (commit.Changes.Any())
            {
                html.AppendLine($"<p><strong>Changes:</strong></p>");
                html.AppendLine("<ul>");
                foreach (var change in commit.Changes)
                {
                    html.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(change)}</li>");
                }
                html.AppendLine("</ul>");
            }

            html.AppendLine("<hr>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return await Task.FromResult(html.ToString());
    }

    private async Task<string> GenerateTextDocumentationAsync(List<GitCommitInfo> commits)
    {
        var text = new System.Text.StringBuilder();

        text.AppendLine("GIT COMMIT DOCUMENTATION");
        text.AppendLine("========================");
        text.AppendLine();
        text.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        text.AppendLine($"Total commits: {commits.Count}");
        text.AppendLine();

        foreach (var commit in commits.OrderByDescending(c => c.Date))
        {
            text.AppendLine($"COMMIT: {commit.Hash[..8]}");
            text.AppendLine($"Author: {commit.Author} <{commit.AuthorEmail}>");
            text.AppendLine($"Date: {commit.Date:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine();
            text.AppendLine("Message:");
            text.AppendLine(commit.Message);
            text.AppendLine();

            if (commit.ChangedFiles.Any())
            {
                text.AppendLine("Changed Files:");
                foreach (var file in commit.ChangedFiles)
                {
                    text.AppendLine($"  - {file}");
                }
                text.AppendLine();
            }

            if (commit.Changes.Any())
            {
                text.AppendLine("Changes:");
                foreach (var change in commit.Changes)
                {
                    text.AppendLine($"  - {change}");
                }
                text.AppendLine();
            }

            text.AppendLine("----------------------------------------");
            text.AppendLine();
        }

        return await Task.FromResult(text.ToString());
    }

    // New methods for remote branch support
    public async Task<List<string>> GetLocalBranchesAsync(string repositoryPath)
    {
        try
        {
            _logger.LogInformation("Getting local branches from repository: {RepositoryPath}", repositoryPath);

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            using var repo = new Repository(repositoryPath);
            var localBranches = repo.Branches
                .Where(b => !b.IsRemote)
                .Select(b => b.FriendlyName)
                .ToList();

            _logger.LogInformation("Found {Count} local branches", localBranches.Count);
            return await Task.FromResult(localBranches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local branches from repository: {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    public async Task<List<string>> GetRemoteBranchesAsync(string repositoryPath)
    {
        try
        {
            _logger.LogInformation("Getting remote branches from repository: {RepositoryPath}", repositoryPath);

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            using var repo = new Repository(repositoryPath);
            var remoteBranches = repo.Branches
                .Where(b => b.IsRemote)
                .Select(b => b.FriendlyName)
                .ToList();

            _logger.LogInformation("Found {Count} remote branches", remoteBranches.Count);
            return await Task.FromResult(remoteBranches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remote branches from repository: {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    public async Task<List<string>> GetAllBranchesAsync(string repositoryPath)
    {
        try
        {
            _logger.LogInformation("Getting all branches from repository: {RepositoryPath}", repositoryPath);

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            using var repo = new Repository(repositoryPath);
            var allBranches = repo.Branches
                .Select(b => b.IsRemote ? $"remote/{b.FriendlyName}" : $"local/{b.FriendlyName}")
                .ToList();

            _logger.LogInformation("Found {Count} total branches", allBranches.Count);
            return await Task.FromResult(allBranches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all branches from repository: {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    public async Task<bool> FetchFromRemoteAsync(string repositoryPath, string remoteName = "origin")
    {
        try
        {
            _logger.LogInformation("Fetching from remote {RemoteName} in repository: {RepositoryPath}", remoteName, repositoryPath);

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            using var repo = new Repository(repositoryPath);
            var remote = repo.Network.Remotes[remoteName];

            if (remote == null)
            {
                _logger.LogWarning("Remote {RemoteName} not found in repository", remoteName);
                return false;
            }

            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remoteName, refSpecs, null, "Fetch from MCP server");

            _logger.LogInformation("Successfully fetched from remote {RemoteName}", remoteName);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from remote {RemoteName} in repository: {RepositoryPath}", remoteName, repositoryPath);
            return await Task.FromResult(false);
        }
    }

    public async Task<List<GitCommitInfo>> GetGitLogsBetweenBranchesWithRemoteAsync(string repositoryPath, string branch1, string branch2, bool fetchRemote = true)
    {
        try
        {
            _logger.LogInformation("Getting git logs between branches {Branch1} and {Branch2} with remote support", branch1, branch2);

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"Path is not a valid git repository: {repositoryPath}");
            }

            // Fetch from remote if requested
            if (fetchRemote)
            {
                await FetchFromRemoteAsync(repositoryPath);
            }

            var commits = new List<GitCommitInfo>();

            using var repo = new Repository(repositoryPath);

            // Handle remote branch references
            var branch1Name = NormalizeBranchName(branch1);
            var branch2Name = NormalizeBranchName(branch2);

            var branch1Ref = repo.Branches[branch1Name] ?? repo.Branches[$"origin/{branch1Name}"];
            var branch2Ref = repo.Branches[branch2Name] ?? repo.Branches[$"origin/{branch2Name}"];

            if (branch1Ref == null)
            {
                throw new ArgumentException($"Branch '{branch1}' not found (tried local and origin remote)");
            }

            if (branch2Ref == null)
            {
                throw new ArgumentException($"Branch '{branch2}' not found (tried local and origin remote)");
            }

            var filter = new CommitFilter
            {
                ExcludeReachableFrom = branch1Ref.Tip,
                IncludeReachableFrom = branch2Ref.Tip
            };

            var repoCommits = repo.Commits.QueryBy(filter);

            foreach (var commit in repoCommits)
            {
                var commitInfo = await CreateGitCommitInfoAsync(repo, commit);
                commits.Add(commitInfo);
            }

            _logger.LogInformation("Retrieved {Count} commits between branches with remote support", commits.Count);
            return commits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git logs between branches {Branch1} and {Branch2} with remote support", branch1, branch2);
            throw;
        }
    }

    private string NormalizeBranchName(string branchName)
    {
        // Remove common prefixes if present
        if (branchName.StartsWith("origin/"))
            return branchName.Substring(7);
        if (branchName.StartsWith("remote/origin/"))
            return branchName.Substring(14);
        if (branchName.StartsWith("local/"))
            return branchName.Substring(6);

        return branchName;
    }

    private int CountLines(TreeEntry entry)
    {
        if (entry == null || entry.TargetType != TreeEntryTargetType.Blob)
            return 0;

        var blob = (Blob)entry.Target;
        if (blob.IsBinary)
            return 0;

        using var contentStream = new StreamReader(blob.GetContentStream());
        var content = contentStream.ReadToEnd();
        return content.Split('\n').Length;
    }

    private List<LineDiff> ParseUnifiedDiff(string unifiedDiff)
    {
        var result = new List<LineDiff>();
        if (string.IsNullOrEmpty(unifiedDiff))
            return result;

        var lines = unifiedDiff.Split('\n');
        var lineNumber = 1;

        // Skip the header lines
        var i = 0;
        while (i < lines.Length && !lines[i].StartsWith("@@"))
            i++;

        // Process diff chunks
        while (i < lines.Length)
        {
            if (lines[i].StartsWith("@@"))
            {
                // Parse the hunk header like @@ -1,7 +1,7 @@
                var match = System.Text.RegularExpressions.Regex.Match(
                    lines[i], @"@@ -(\d+)(?:,\d+)? \+(\d+)(?:,\d+)? @@");

                if (match.Success)
                {
                    var oldStart = int.Parse(match.Groups[1].Value);
                    var newStart = int.Parse(match.Groups[2].Value);

                    result.Add(new LineDiff
                    {
                        LineNumber = lineNumber++,
                        OldLineNumber = "-",
                        NewLineNumber = "-",
                        Content = lines[i],
                        Type = "Header"
                    });

                    i++;
                    var oldLine = oldStart;
                    var newLine = newStart;

                    // Process the lines in this hunk
                    while (i < lines.Length && !lines[i].StartsWith("@@"))
                    {
                        if (lines[i].StartsWith("+"))
                        {
                            // Added line
                            result.Add(new LineDiff
                            {
                                LineNumber = lineNumber++,
                                OldLineNumber = "-",
                                NewLineNumber = newLine.ToString(),
                                Content = lines[i],
                                Type = "Added"
                            });
                            newLine++;
                        }
                        else if (lines[i].StartsWith("-"))
                        {
                            // Deleted line
                            result.Add(new LineDiff
                            {
                                LineNumber = lineNumber++,
                                OldLineNumber = oldLine.ToString(),
                                NewLineNumber = "-",
                                Content = lines[i],
                                Type = "Deleted"
                            });
                            oldLine++;
                        }
                        else if (!string.IsNullOrEmpty(lines[i]))
                        {
                            // Context line
                            result.Add(new LineDiff
                            {
                                LineNumber = lineNumber++,
                                OldLineNumber = oldLine.ToString(),
                                NewLineNumber = newLine.ToString(),
                                Content = lines[i],
                                Type = "Context"
                            });
                            oldLine++;
                            newLine++;
                        }

                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
            else
            {
                i++;
            }
        }

        return result;
    }
}
