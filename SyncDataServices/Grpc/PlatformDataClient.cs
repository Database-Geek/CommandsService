using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService;
using ILogger = Serilog.ILogger;

namespace CommandsService.SyncDataServices.Grpc
{
  public class PlatformDataClient : IPlatformDataClient
  {
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public PlatformDataClient(ILogger logger, IConfiguration configuration, IMapper mapper)
    {
      _logger = logger;
      _configuration = configuration;
      _mapper = mapper;


      _logger.Information("--> Starting PlatformDataClient.");
    }

    public IEnumerable<Platform> ReturnAllPlatforms()
    {
      _logger.Information("--> Calling GRPC Service: {grpcService}", _configuration["GrpcPlatform"]);

      var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
      var client = new GrpcPlatform.GrpcPlatformClient(channel);
      var request = new GetAllRequest();

      try
      {
        var reply = client.GetAllPlatforms(request);
        return _mapper.Map<IEnumerable<Platform>>(reply.Platform);
      }
      catch (Exception ex)
      {
        _logger.Warning("--> Could not call GRPC server: {errorMessage}", ex.Message);
        return null;
      }
    }
  }  
}