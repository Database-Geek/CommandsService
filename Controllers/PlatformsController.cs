using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;

namespace CommandsService.Controllers
{
  [Route("api/c/[controller]")]
  [ApiController]
  public class PlatformsController : ControllerBase
  {
    private readonly ILogger _logger;
    public PlatformsController(ILogger logger)
    {
      _logger = logger;
    }

    public ActionResult TestInboundConnection()
    {
      string message = "--> Inbound POST # Command Service";
      _logger.Information(message);

      return Ok("Inbound test of from Platforms Controller");
    }

  }  
}