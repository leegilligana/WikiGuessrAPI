using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSessionController(IManageGameSessions gameSessionManager) : ControllerBase
{
    [HttpPost(Name = "CreateGame")]
    public async Task<ActionResult<string>> CreateGame(int numRounds, string hostPlayerName)
    {
        var (sessionGuid, hostGuid) = await gameSessionManager.CreateNewGameSessionAsync(numRounds, hostPlayerName);

        return Ok($"Game session created successfully. Session id: {sessionGuid}, player id: {hostGuid}");
    }

    [HttpGet("{sessionId}/exists", Name = "CheckGameExists")]
    public async Task<ActionResult<bool>> CheckGameExists(Guid sessionId)
    {
        var exists = await gameSessionManager.DoesGameSessionExistAsync(sessionId);

        return Ok(exists);
    }

    [HttpDelete("{sessionId}", Name = "DeleteGame")]
    public async Task<ActionResult> DeleteGame(Guid sessionId)
    {
        await gameSessionManager.DeleteSessionIfExistsAsync(sessionId);
        return Ok($"Game session {sessionId} deleted successfully.");
    }
}
