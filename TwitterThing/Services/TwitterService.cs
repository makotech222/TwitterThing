using TwitterThing.Models;
using TwitterThing.Utilities;

namespace TwitterThing.Services
{
    public class TwitterService
    {
        private const string _twitterUrl = "https://api.twitter.com/2/tweets/sample/stream";
        private CancellationTokenSource _cancellationTokenSource;
        private HttpClient _httpClient;
        private TwitterStatistics _twitterStatistics;

        public TwitterService(string bearerToken, TwitterStatistics statistics)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = new TimeSpan(0, 0, 15);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
            _cancellationTokenSource = new CancellationTokenSource();
            _twitterStatistics = statistics;
        }

        public async Task BeginProcess()
        {
            while (true)
            {
                try
                {
                    using (var response = await _httpClient.GetStreamAsync(_twitterUrl, _cancellationTokenSource.Token))
                    {
                        await PipeLineReader.ProcessLinesAsync(response, ProcessLine, _cancellationTokenSource.Token);
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("Invalid bearer token.");
                        throw;
                    }
                    else
                    {
                        Console.WriteLine("Unknown network error. Retrying connection in 10 seconds.");
                        await Task.Delay(10 * 1000);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private void ProcessLine(string s)
        {
            if (s != "\r") // Occasionally get some \r lines in the stream.
            {
                _twitterStatistics.AddTweetToQueue(s);
            }
        }
    }
}