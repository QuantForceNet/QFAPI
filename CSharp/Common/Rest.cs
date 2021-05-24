using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuantForce
{
    public static class Extension
    {
        public static string AppendToURL(this string baseURL, params string[] segments)
        {
            {
                return string.Join("/", new[] { baseURL.TrimEnd('/') }.Concat(segments.Select(s => s.Trim('/'))));
            }
        }
        public static Uri Append(this Uri uri, params string[] segments)
        {
            return new Uri(Append_ToAbsoluteUriString(uri, segments));
        }
        public static string Append_ToAbsoluteUriString(this Uri uri, string[] segments)
        {
            return AppendToURL(uri.AbsoluteUri, segments);
        }
    }
        public class Rest
    {
        public HttpClient Client()
        {
            var result = new HttpClient();
            foreach (var h in Headers)
                result.DefaultRequestHeaders.Add(h.Key, h.Value);
            return result;
        }

        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string query, string body = null)
        {
            var request = new HttpRequestMessage(method, query);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var client = Client();

            var response = await client.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Request failed:" + await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            return response;
        }
        public async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string query, byte[] body)
        {
            var request = new HttpRequestMessage(method, query);
            if (body != null)
                request.Content = new ByteArrayContent(body);
            var client = Client();

            var response = await client.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Request failed:" + await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            return response;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await SendAsync(HttpMethod.Get, uri);
            response.EnsureSuccessStatusCode();
            string s = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(s);
            return JsonConvert.DeserializeObject<T>(s);
        }

        public async Task GetAsync(string uri)
        {
            var response = await SendAsync(HttpMethod.Get, uri);
            response.EnsureSuccessStatusCode();
        }

        public async Task DownloadAsync(string uri, string destination)
        {
            var response = await SendAsync(HttpMethod.Get, uri);
            response.EnsureSuccessStatusCode();
            using (var file = System.IO.File.OpenWrite(destination))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(file);
            }
        }

        public async Task<T> PostAsync<T>(string uri, object data = null)
        {
            var response = await SendAsync(HttpMethod.Post, uri, JsonConvert.SerializeObject(data));
            response.EnsureSuccessStatusCode();
            var s = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(s);
        }
        public async Task<T> PostRawAsync<T>(string uri, byte[] data)
        {
            var response = await SendRawAsync(HttpMethod.Post, uri, data);
            response.EnsureSuccessStatusCode();
            var s = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(s);
        }

        public async Task<T> PutAsync<T>(string uri, object data = null)
        {
            var response = await SendAsync(HttpMethod.Put, uri, JsonConvert.SerializeObject(data));
            response.EnsureSuccessStatusCode();
            var s = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}
