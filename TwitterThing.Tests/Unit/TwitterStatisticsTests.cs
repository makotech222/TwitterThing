using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using TwitterThing.Models;
using static TwitterThing.Models.TwitterStatistics;

namespace TwitterThing.Tests.Unit
{
    [TestClass]
    public class TwitterStatisticsTests
    {
        [TestMethod]
        public void CanAddToQueue()
        {
            var stats = new TwitterStatistics();
            for (int i = 0; i < 1000; i++)
            {
                stats.AddTweetToQueue("");
            }
        }

        [TestMethod]
        public async Task CanReadFromMultipleThreads()
        {
            var stats = new TwitterStatistics();
            var model = new TweetInfo() { data = new TweetInfo.Data() { id = "1", text = "test", edit_history_tweet_ids = new List<string>() } };
            var tasks = new List<Task>();
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    stats.AddTweetToQueue(JsonSerializer.Serialize(model));
                }
            }));

            for (int i = 0; i < 32; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var result = stats.GetStatistics();
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        [TestMethod]
        public void CanReadHashtags()
        {
            var stats = new TwitterStatistics();
            var model = new TweetInfo() { data = new TweetInfo.Data() { id = "1", text = "test #test #hash2 #hash3 # hash4 #_hash5", edit_history_tweet_ids = new List<string>() } };
            stats.AddTweetToQueue(JsonSerializer.Serialize(model));

            var result = stats.GetStatistics();
            Assert.IsTrue(result.Item2.Count == 4);
        }

        [TestMethod]
        public void CanCountTweets()
        {
            var stats = new TwitterStatistics();
            var model = new TweetInfo() { data = new TweetInfo.Data() { id = "1", text = "test #test #hash2 #hash3 # hash4 #_hash5", edit_history_tweet_ids = new List<string>() } };
            for (int i = 0; i < 1500; i++)
            {
                stats.AddTweetToQueue(JsonSerializer.Serialize(model));
            }

            var result = stats.GetStatistics();
            Assert.IsTrue(result.Item1 == 1500);
        }
    }
}