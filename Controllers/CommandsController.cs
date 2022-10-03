using AutoMapper;
using CommandService.Models;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace CommandsService.Controllers
{
  [Route("api/c/platforms/{platformId}/[controller]")]
  [ApiController]
  public class CommandsController : ControllerBase
  {
    private readonly ILogger _logger;
    private readonly ICommandRepo _repository;
    private readonly IMapper _mapper;

    public CommandsController(ILogger logger, ICommandRepo repository, IMapper mapper)
    {
      _logger = logger;
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
    {
      _logger.Information("--> Hit GetCommandsForPlatform : {platformId}", platformId);
      if (!_repository.PlatformExists(platformId))
      {
        return NotFound();
      }

      var commands = _repository.GetCommandsForPlatform(platformId);

      return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
    }

    [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
    public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
    {
      _logger.Information("--> Hit GetCommandForPlatform : {platformId} / {commandId}", platformId, commandId);
      if (!_repository.PlatformExists(platformId))
      {
        return NotFound();
      }

      var command = _repository.GetCommand(platformId, commandId);
      
      if (command == null)
      {
        return NotFound();
      }

      return Ok(_mapper.Map<CommandReadDto>(command));
    }
    
    [HttpPost]
    public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
    {
      _logger.Information("--> Hit CreateCommandForPlatform : {platformId}", platformId);
      if (!_repository.PlatformExists(platformId))
      {
        return NotFound();
      }

      var command = _mapper.Map<Command>(commandDto);

      _repository.CreateCommand(platformId, command);
      _repository.SaveChanges();

      var commandReadDto = _mapper.Map<CommandReadDto>(command);

      return CreatedAtRoute(nameof(GetCommandForPlatform),
                              new { platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
    }

  }
}