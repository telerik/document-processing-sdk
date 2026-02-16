using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using Telerik.Documents.AI.AgentTools.Fixed;
using Telerik.Documents.AI.AgentTools.Spreadsheet;
using Telerik.Documents.AI.Tools.Fixed.Core;
using Telerik.Documents.AI.Tools.Spreadsheet.Core;

namespace Telerik.Documents.AI.AgentTools.Examples;

/// <summary>
/// Example: Controller with per-user document isolation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class DocumentChatController : ControllerBase
{
    private readonly IChatClient _chatClient;

    public DocumentChatController(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    /// <summary>
    /// Process a chat message with document tools.
    /// Each user has isolated document repositories.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        // ============================================================
        // STEP 1: Get user identity from your auth system
        // ============================================================
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // ============================================================
        // STEP 2: Get or create user-specific repositories
        // These persist across requests for the same user
        // ============================================================
        var session = UserSessionManager.GetOrCreateSession(userId);

        // ============================================================
        // STEP 3: Create tools with THIS user's repositories
        // ============================================================

        // imageDirectory is where extracted images from PDFs will be stored
        var filesDirectory = Path.Combine("UserData", userId, "Files");
        Directory.CreateDirectory(filesDirectory);

        var spreadsheetReadTools = new SpreadProcessingReadAgentTools(session.WorkbookRepository);
        var spreadsheetWriteTools = new SpreadProcessingWriteAgentTools(session.WorkbookRepository);
        var spreadsheetFileTools = new SpreadProcessingFileManagementAgentTools(session.WorkbookRepository, filesDirectory);

        // imageDirectory is where extracted images from PDFs will be stored
        var imageDirectory = Path.Combine("UserData", userId, "Images");
        Directory.CreateDirectory(imageDirectory);
        var pdfContentTools = new FixedDocumentContentAgentTools(session.PdfRepository, imageDirectory);

        var allTools = spreadsheetReadTools.GetTools()
            .Concat(spreadsheetWriteTools.GetTools())
            .Concat(spreadsheetFileTools.GetTools())
            .Concat(pdfContentTools.GetTools())
            .ToList();

        // ============================================================
        // STEP 4: Process with AI
        // ============================================================

        AIAgent agent = this._chatClient.AsAIAgent(
                    instructions: "",
                    name: "Analyzer",
                    tools: allTools);

        AgentResponse response = await agent.RunAsync(new ChatMessage(ChatRole.User, request.Message));

        return Ok(new ChatResponse { Message = response.Text });
    }

    /// <summary>
    /// List documents in the current user's repository.
    /// </summary>
    [HttpGet("documents")]
    public IActionResult ListDocuments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var session = UserSessionManager.GetOrCreateSession(userId);

        var documents = new
        {
            spreadsheets = session.WorkbookRepository.ListDocuments().Select(d => d.Id),
            pdfs = session.PdfRepository.ListDocuments().Select(d => d.Id)
        };

        return Ok(documents);
    }

    /// <summary>
    /// Clear all documents for the current user.
    /// </summary>
    [HttpDelete("documents")]
    public IActionResult ClearDocuments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        UserSessionManager.ClearSession(userId);
        return Ok(new { message = "All documents cleared" });
    }

    /// <summary>
    /// End the user's session and clean up resources.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        UserSessionManager.ClearSession(userId);

        // Optionally clean up user's files
        var userDataDir = Path.Combine("UserData", userId);
        if (Directory.Exists(userDataDir))
        {
            Directory.Delete(userDataDir, recursive: true);
        }

        return Ok(new { message = "Session ended" });
    }
}

/// <summary>
/// Manages user sessions with their associated document repositories.
/// Thread-safe for concurrent access.
/// </summary>
public static class UserSessionManager
{
    private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    /// <summary>
    /// Gets an existing session or creates a new one for the user.
    /// </summary>
    public static UserSession GetOrCreateSession(string userId)
    {
        var session = _sessions.GetOrAdd(userId, _ => new UserSession
        {
            UserId = userId,
            WorkbookRepository = new InMemoryWorkbookRepository(importTimeout: null),
            PdfRepository = new InMemoryFixedDocumentRepository(importTimeout: null),
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        });

        // Update last accessed time on every access
        session.LastAccessedAt = DateTime.UtcNow;
        return session;
    }

    /// <summary>
    /// Clears and removes a user's session.
    /// </summary>
    public static void ClearSession(string userId)
    {
        if (_sessions.TryRemove(userId, out var session))
        {
            session.Dispose();
        }
    }

    /// <summary>
    /// Cleans up sessions that haven't been accessed recently.
    /// Call this periodically (e.g., from a background service).
    /// </summary>
    public static void CleanupStaleSessions(TimeSpan maxIdleTime)
    {
        var cutoff = DateTime.UtcNow - maxIdleTime;
        var staleUserIds = _sessions
            .Where(kvp => kvp.Value.LastAccessedAt < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var userId in staleUserIds)
        {
            ClearSession(userId);
        }
    }
}

/// <summary>
/// Represents a user's session with their isolated document repositories.
/// </summary>
public class UserSession : IDisposable
{
    public string UserId { get; init; } = string.Empty;
    public IWorkbookRepository WorkbookRepository { get; init; } = null!;
    public IFixedDocumentRepository PdfRepository { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime LastAccessedAt { get; set; }

    public void Dispose()
    {
        // Dispose repositories if they implement IDisposable
        (WorkbookRepository as IDisposable)?.Dispose();
        (PdfRepository as IDisposable)?.Dispose();
    }
}

/// <summary>
/// Background service to clean up stale user sessions.
/// Register in DI: services.AddHostedService&lt;SessionCleanupService&gt;();
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _maxIdleTime = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, stoppingToken);
            UserSessionManager.CleanupStaleSessions(_maxIdleTime);
        }
    }
}

/// <summary>
/// Chat request model.
/// </summary>
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Chat response model.
/// </summary>
public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
}
