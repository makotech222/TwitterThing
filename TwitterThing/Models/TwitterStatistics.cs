using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TwitterThing.Models
{
    /// <summary>
    /// Stores and calculates important statistics from the Twitter stream
    /// </summary>
    public class TwitterStatistics
    {
        private int _tweetCount = 0;
        private Dictionary<string, int> _hashtagCount = new Dictionary<string, int>();
        private ConcurrentQueue<string> _tweetInfoJSONQueue = new ConcurrentQueue<string>();
        private Regex _hashtagRegex = new Regex(@"#\w+");

        public TwitterStatistics()
        {
        }

        public void AddTweetToQueue(string tweetInfo)
        {
            Console.WriteLine(tweetInfo);
            _tweetInfoJSONQueue.Enqueue(tweetInfo);
        }

        /// <summary>
        /// Calculate statistics on available tweets. Returns number of tweets and top 10 hashtags
        /// </summary>
        /// <returns></returns>
        public (int, List<string>) GetStatistics()
        {
            var count = _tweetInfoJSONQueue.Count;
            for (int i = 1; i <= count; i++)
            {
                string str = null;
                if (_tweetInfoJSONQueue.TryDequeue(out str))
                {
                    TweetInfo tweetInfo = null;
                    try
                    {
                        tweetInfo = JsonSerializer.Deserialize<TweetInfo>(str, JsonSerializerOptions.Default);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Failed to deserialize text. Skipping entry. String: {str}");
                        continue;
                    }
                    _tweetCount++;

                    var matches = _hashtagRegex.Matches(tweetInfo.data.text).ToList();
                    foreach (var match in matches)
                    {
                        if (_hashtagCount.ContainsKey(match.Value))
                        {
                            _hashtagCount[match.Value]++;
                        }
                        else
                        {
                            _hashtagCount.Add(match.Value, 1);
                        }
                    }
                }
            }
            return (_tweetCount, _hashtagCount.OrderByDescending(x => x.Value).Take(10).Select(y => y.Key).ToList());
        }
    }
}