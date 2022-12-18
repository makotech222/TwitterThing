using TwitterThing.Models;
using TwitterThing.Services;
using TwitterThing.Utilities;

namespace TwitterThing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            var twitterBearerToken = Environment.GetEnvironmentVariable("TwitterBearerToken", EnvironmentVariableTarget.User).NullIfEmpty() ?? app.Configuration.GetValue<string>("TwitterBearerToken").NullIfEmpty() ?? throw new Exception("No twitter bearer token found.");
            var twitterStatistics = new TwitterStatistics();

            app.MapGet("/", (HttpContext httpContext) =>
            {
                var result = twitterStatistics.GetStatistics();
                return Results.Json(new { NumberOfTweets = result.Item1, TopTenHashtags = result.Item2 });
            });

            //Start long running task to stream from twitter
            Task.Factory.StartNew(async () =>
            {
                TwitterService t = new TwitterService(twitterBearerToken, twitterStatistics);

                app.Lifetime.ApplicationStopping.Register(() => { t.Cancel(); });
                try
                {
                    await t.BeginProcess();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An exception has occurred. Closing application. Exception: {e}");
                    app.Lifetime.StopApplication();
                }
            }, TaskCreationOptions.LongRunning);

            app.Run();
        }
    }
}