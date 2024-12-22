using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiCallHandler
{
    /// <summary>
    /// Handles API calls with state management and caching functionality.
    /// </summary>
    public class ApiHandler : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, ApiResponseCache> _cache;

        /// <summary>
        /// Initializes a new instance of the ApiHandler class.
        /// </summary>
        public ApiHandler()
        {
            _httpClient = new HttpClient();
            _cache = new Dictionary<string, ApiResponseCache>();
        }

        /// <summary>
        /// Sends a GET request to the specified endpoint and manages response state.
        /// </summary>
        /// <param name="url">The API endpoint URL.</param>
        /// <param name="useCache">Indicates whether to use cached responses.</param>
        /// <returns>The API response as a string.</returns>
        public async Task<string> GetAsync(string url, bool useCache = true)
        {
            if (useCache && _cache.ContainsKey(url))
            {
                var cachedResponse = _cache[url];
                if (!cachedResponse.IsExpired)
                {
                    return cachedResponse.Content;
                }
            }

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();

                // Cache the response
                _cache[url] = new ApiResponseCache(content);
                return content;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a POST request with the specified payload.
        /// </summary>
        /// <param name="url">The API endpoint URL.</param>
        /// <param name="payload">The payload to be sent.</param>
        /// <typeparam name="T">The type of the payload.</typeparam>
        /// <returns>The API response as a string.</returns>
        public async Task<string> PostAsync<T>(string url, T payload)
        {
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Clears the cached responses.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Disposes the HttpClient and clears resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Represents a cached API response.
    /// </summary>
    internal class ApiResponseCache
    {
        private readonly DateTime _timestamp;
        public string Content { get; }

        /// <summary>
        /// Time-to-live for cache in seconds.
        /// </summary>
        private const int CacheTtl = 60;

        public ApiResponseCache(string content)
        {
            Content = content;
            _timestamp = DateTime.Now;
        }

        /// <summary>
        /// Checks if the cached response has expired.
        /// </summary>
        public bool IsExpired => (DateTime.Now - _timestamp).TotalSeconds > CacheTtl;
    }
}