using CommandService.Models;
using CommandsService.SyncDataServices.Grpc;
using ILogger = Serilog.ILogger;

namespace CommandsService.Data
{
  public static class PrepDb
  {
    public static void PrepPopulation(ILogger logger, IApplicationBuilder app)
    {
      using (var serviceScope = app.ApplicationServices.CreateScope())
      {
        var grpClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

        var platforms = grpClient.ReturnAllPlatforms();

        SeedData(logger, serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
      }
    }

    private static void SeedData(ILogger logger, ICommandRepo repo, IEnumerable<Platform> platforms)
    {
      logger.Information("--> Seeding new platforms...");

      foreach(var platform in platforms)
      {
        if(!repo.ExternalPlatformExists(platform.ExternalId))
        {
          repo.CreatePlatform(platform);
        }
        repo.SaveChanges();
      }
    }
  }
}