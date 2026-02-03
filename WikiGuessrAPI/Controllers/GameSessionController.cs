using Microsoft.AspNetCore.Mvc;
using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameSessionController : ControllerBase
{
    [HttpPost(Name = "JoinGame")]
    public ActionResult<string> JoinGame(string? gameCode) => Ok(gameCode);
}
