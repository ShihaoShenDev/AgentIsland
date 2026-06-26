namespace AgentIsland.Models;

/// <summary>
/// MCP 服务器传输模式
/// </summary>
public enum McpTransportMode
{
    /// <summary>
    /// Streamable HTTP（现代传输协议）
    /// </summary>
    StreamableHttp,

    /// <summary>
    /// SSE（Server-Sent Events，旧版传输协议）
    /// </summary>
    Sse
}
