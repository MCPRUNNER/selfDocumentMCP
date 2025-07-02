# selfDocumentMCP Copilot Instructions

This is a Model Context Protocol (MCP) Server project that contains MCP tools using JSON-RPC 2.0 and git logs to create documentation, and is designed to be used by vscode as a Copilot Agent.

# You can find more info and examples at

## References

- https://modelcontextprotocol.io/llms-full.txt
- https://github.com/modelcontextprotocol/csharp-sdk

This project:

- Implements the Model Context Protocol Server for STDIO and JSON-RPC 2.0
- gets git logs and creates documentation
- Uses the ModelContextProtocol for communication
- Allows VS Code and Copilot to connect to it via a JSON-RPC 2.0 interface and STDIO in
- Supports fetching metadata
- Uses C# for the implementation
- To be used as a Copilot Agent MCP
- has a tool that generates documentation from git logs
- has a tool that generates documentation from git logs and writes it to a file
- has a tool that generates documentation from git logs and writes it to a file in the current workspace
- has a tool that generates documentation from git logs and writes it to a file in the current workspace with a specific filename
- has a tool that generates documentation from git logs compares differences between two branches and writes it to a file in the current workspace with a specific filename
- has a tool that generates documentation from git logs compares differences between two commits and writes it to a file in the current workspace with a specific filename
- output includes the commit message, author, date, and changes made
- output is formatted in a human-readable way
- output is formatted in a way that can be used by Copilot

# Additional Instructions

- Use current vscode workspace
- Use ModelContextProtocol
- Use a logging framework for logging
- Use a configuration file for connection strings
- Add all changes to current workspace
- Use a .gitignore file to ignore build files
- Create example mcp.json for running with Copilot Agent in VS Code
- Make sure to include an initialize endpoint.
- Use best practices
- when prompting for terminal commands, use a semicolon to separate commands
