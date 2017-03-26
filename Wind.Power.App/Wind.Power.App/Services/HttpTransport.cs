using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Wind.Power.App.Services
{
    public class HttpTransport
    {
        private readonly string baseUrl;

        public HttpTransport(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public async Task<T> Get<T>(string url, Dictionary<string, object> parameters = null)
        {
            var client = GetClient();

            if (parameters != null)
            {
                bool isFirst = true;
                foreach (var p in parameters)
                {
                    if (isFirst)
                    {
                        url += $"?{p.Key}={p.Value}";
                    }
                    else
                    {
                        url += $"&{p.Key}={p.Value}";
                    }
                }
            }

            try
            {
                var requestTask = client.GetAsync(new Uri(url));
                var response = Task.Run(() => requestTask).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Responseis not ok for url=" + url + " " + response.ReasonPhrase);
                    return default(T);
                }

                var data = await response.Content.ReadAsStringAsync();

                var returnObject = JsonConvert.DeserializeObject<T>(data);
                return returnObject;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while GET data. for url=" + url + "\n" + ex.ToString());
                return default(T); ;
            }
        }

        

        public async Task<T> Post<T>(string url, Object body)
        {
            var client = GetClient();
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(new Uri(url), content);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Responseis not ok for url=" + url + " " + response.ReasonPhrase);
                    return default(T);
                }

                var data = await response.Content.ReadAsStringAsync();

                var returnObject = JsonConvert.DeserializeObject<T>(data);
                return returnObject;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while POST data. for url=" + url + "\n" + ex.ToString());
                return default(T); ;
            }
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            return client;
        }
    }
}
