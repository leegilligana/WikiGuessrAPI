using Microsoft.AspNetCore.Mvc;
using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
internal class GuessController : ControllerBase
{
    [HttpPost(Name = "PostGuess")]
    public ActionResult PostGuess([FromBody] GuessSubmission guess) => Ok();

    [HttpGet(Name = "GetGuess")]
    public ActionResult Get() => Ok();
}
