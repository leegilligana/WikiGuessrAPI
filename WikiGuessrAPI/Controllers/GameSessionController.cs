using Microsoft.AspNetCore.Mvc;
using WikiGuessrAPI.Models.DTO;
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

    [HttpPost("{sessionId}/players", Name = "AddPlayerToSession")]
    public async Task<ActionResult<Guid>> AddPlayerToSessionAsync([FromRoute] Guid sessionId, [FromQuery] string playerName)
    {
        var playerId = await gameSessionManager.AddPlayerToSessionAsync(sessionId, playerName);
        return Ok(playerId);
    }

    [HttpGet("{sessionId}", Name = "FetchSession")]
    public async Task<ActionResult<SessionDTO>> FetchSessionAsync([FromRoute] Guid sessionId)
    {
        var session = await gameSessionManager.FetchSessionAsync(sessionId);
        var dto = SessionDTO.FromSession(session);

        return Ok(dto);
    }

    [HttpDelete("{sessionId}", Name = "DeleteSession")]
    public async Task<ActionResult> DeleteSessionAsync([FromRoute] Guid sessionId)
    {
        await gameSessionManager.DeleteSessionIfExistsAsync(sessionId);
        return Ok($"Game session {sessionId} deleted successfully.");
    }

    [HttpDelete("{sessionId}/players/{playerId}", Name = "RemovePlayerFromSession")]
    public async Task<ActionResult> RemovePlayerFromSessionAsync([FromRoute] Guid sessionId, [FromRoute] Guid playerId)
    {
        await gameSessionManager.RemovePlayerFromSessionAsync(sessionId, playerId);
        return Ok($"Player {playerId} removed from session {sessionId} successfully.");
    }

    [HttpDelete("{sessionId}/hosts/{hostId}", Name = "DeleteSessionIfHost")]
    public async Task<ActionResult> DeleteSessionIfHostAsync([FromRoute] Guid sessionId, [FromRoute] Guid hostId)
    {
        await gameSessionManager.DeleteSessionIfHostAsync(sessionId, hostId);
        return Ok($"Game session {sessionId} deleted successfully by host {hostId}.");
    }

    [HttpDelete("{sessionId}/hosts/{hostId}/playerNames/{playerName}", Name = "RemovePlayerIfHost")]
    public async Task<ActionResult> RemovePlayerIfHostAsync([FromRoute] Guid sessionId, [FromRoute] Guid hostId, [FromRoute] string playerName)
    {
        await gameSessionManager.RemovePlayerIfHostAsync(sessionId, hostId, playerName);
        return Ok($"Player {playerName} removed from session {sessionId} successfully by host {hostId}.");
    }
}
