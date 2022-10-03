using AutoMapper;
using CommandService.Models;
using CommandsService.Data;
using CommandsService.Dtos;
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
    private readonly ICommandRepo _repository;
    private readonly IMapper _mapper;

    public PlatformsController(ILogger logger, ICommandRepo repository, IMapper mapper)
    {
      _logger = logger;
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Platform>> GetPlatforms()
    {
      _logger.Information("--> Getting Platforms from CommandService.");

      var platformItems = _repository.GetAllPlatforms();

      return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
    }

    [HttpPost]
    public ActionResult TestInboundConnection()
    {
      string message = "--> Inbound POST # Command Service";
      _logger.Information(message);

      return Ok("Inbound test of from Platforms Controller");
    }

  }  
}