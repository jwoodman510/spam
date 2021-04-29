using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPAM
{
    public class Spammer : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;

        public Spammer(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _httpClient = new HttpClient();
        }

        public void Dispose() => _httpClient.Dispose();

        public async Task SpamAsync()
        {
            if (_appSettings.Requests == null)
            {
                return;
            }

            var metrics = new Metrics();
            var batch = new List<HttpRequestMessage>();

            foreach (var request in GetRequests())
            {
                if (batch.Count == 0)
                {
                    Consoler.WriteLines(ConsoleColor.Cyan, $"Building Batch No. {metrics.BatchNumber}.");
                }

                batch.Add(request);

                if (batch.Count >= _appSettings.BatchSize)
                {
                    await TryProcessBatchAsync(batch, metrics);
                }
            }

            if (batch.Count > 0)
            {
                await TryProcessBatchAsync(batch, metrics);
            }
        }

        private async Task TryProcessBatchAsync(List<HttpRequestMessage> batch, Metrics metrics)
        {
            try
            {
                Consoler.WriteLines(ConsoleColor.Cyan, $"Processing Batch No. {metrics.BatchNumber} | Request Count: {batch.Count}");

                await Task.WhenAll(batch.Select(x => _httpClient.SendAsync(x).ContinueWith(x => x.IsFaulted ? null : x)));
            }
            catch (Exception ex)
            {
                Consoler.WriteError($"Batch No. {metrics.BatchNumber} Failed Unexpectedly.", ex.Message);
            }
            finally
            {
                metrics.ProcessedCount += batch.Count;

                Consoler.WriteLines(ConsoleColor.Green, $"Batch No. {metrics.BatchNumber} Processed.");
                Consoler.WriteLines(ConsoleColor.White, $"Requests Processed: {metrics.ProcessedCount}.");

                metrics.BatchNumber++;

                if (_appSettings.BatchThrottleSeconds > 0)
                {
                    var secs = _appSettings.BatchThrottleSeconds;

                    while (secs > 0)
                    {
                        if (secs == _appSettings.BatchThrottleSeconds)
                        {
                            Consoler.WriteLines(ConsoleColor.Yellow, $"Building next batch in...", secs.ToString());
                        }
                        else
                        {
                            Consoler.WriteLines(ConsoleColor.Yellow, $"{secs}");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));

                        secs--;
                    }
                }

                batch.Clear();
            }
        }

        private IEnumerable<HttpRequestMessage> GetRequests() =>_appSettings.Requests.SelectMany(request => request.Urls.Select(url => BuildHttpRequestMessage(request, url)));

        private static HttpRequestMessage BuildHttpRequestMessage(Request request, string url)
        {
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), url);

            foreach (var header in request.Headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }

            return httpRequestMessage;
        }

        private class Metrics
        {
            public int BatchNumber { get; set; } = 1;

            public int ProcessedCount { get; set; }
        }
    }
}
