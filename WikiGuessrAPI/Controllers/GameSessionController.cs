using Microsoft.AspNetCore.Mvc;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSessionController(IManageGameSessions gameSessionManager) : ControllerBase
{
    [HttpPost(Name = "CreateSession")]
    public async Task<ActionResult> CreateSessionAsync([FromQuery] int numRounds, [FromQuery] string hostPlayerName)
    {
        var (sessionGuid, hostGuid) = await gameSessionManager.CreateNewSessionAsync(numRounds, hostPlayerName);
        var result = new { SessionId = sessionGuid, PlayerId = hostGuid };

        return CreatedAtRoute("FetchSession", new { sessionId = sessionGuid }, result);
    }

    [HttpGet("{sessionId}", Name = "FetchSession")]
    public async Task<ActionResult<bool>> FetchSessionAsync(Guid sessionId)
    {
        var exists = await gameSessionManager.FetchSessionAsync(sessionId);

        return Ok(exists);
    }

    [HttpDelete("{sessionId}", Name = "DeleteSession")]
    public async Task<ActionResult> DeleteSessionAsync(Guid sessionId)
    {
        await gameSessionManager.DeleteSessionIfExistsAsync(sessionId);
        return Ok($"Game session {sessionId} deleted successfully.");
    }
}
