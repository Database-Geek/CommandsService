using System.Text.Json;
using AutoMapper;
using CommandService.Models;
using CommandsService.Data;
using CommandsService.Dtos;
using ILogger = Serilog.ILogger;

namespace CommandsService.EventProcessing
{
  public class EventProcessor : IEventProcessor
  {
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;

    public EventProcessor(ILogger logger, 
                          IServiceScopeFactory scopeFactory,
                          IMapper mapper)
    {
      _logger = logger;
      _scopeFactory = scopeFactory;
      _mapper = mapper;

      _logger.Information("--> Beginning EventProcessor.");
    }


    public void ProcessEvent(string message)
    {
      var eventType = DetermineEvent(message);

      _logger.Information("--> Event Type is: {eventType}", eventType);

      switch (eventType)
      {
        case EventType.PlatformPublished:
          AddPlatform(message);
          break;
        default:
          break;
      }
    }

    private EventType DetermineEvent(string notificationMessage)
    {
      _logger.Information("--> Determining Event.");

      var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

      switch(eventType.Event)
      {
        case "Platform_Published":
          _logger.Information("--> Platform Published Event Detected.");
          return EventType.PlatformPublished;
        default:
          _logger.Information("--> Could not determine the event type.");
          return EventType.Undetermined;
      }
    }

    private void AddPlatform(string platformPublishedMessage)
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

        var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

        try
        {
          var plat = _mapper.Map<Platform>(platformPublishedDto);

          if(!repo.ExternalPlatformExists(plat.ExternalId))
          {
            _logger.Information("--> Adding Platform to DB.");
            repo.CreatePlatform(plat);
            repo.SaveChanges();
          }
          else 
          {
            _logger.Warning("--> Platform already exists.");
          }
        }
        catch (Exception ex)
        {
          _logger.Warning("--> Could not add Platform to DB: {errorMessage}", ex.Message);
        }
      }
    }
  }

  enum EventType
  {
    PlatformPublished,
    Undetermined
  }
}