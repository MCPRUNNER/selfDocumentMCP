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
            var branch1Ref = repo.Branches[branch1];
            var branch2Ref = repo.Branches[branch2];

            if (branch1Ref == null || branch2Ref == null)
            {
                throw new ArgumentException("One or both branches not found");
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
}
