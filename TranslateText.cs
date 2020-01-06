using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace connectitfunction
{
    public static class TranslateText
    {
        [FunctionName("TranslateText")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string input = req.Query["input"];
            string from = req.Query["from"];
            string to = req.Query["to"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            input = input ?? data?.input;

            var body = new object[] { new { Text = input } };
            var translatorBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={from}&to={to}");
                request.Content = new StringContent(translatorBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", "MY-KEY");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TranslationsResult[]> (responseBody);

                try
                {
                    var translation = result[0].translations[0].text;
                    return new OkObjectResult(translation);
                }
                catch
                {
                    return new BadRequestObjectResult("No ");
                }
            }
        }
    }

    public class TranslationsResult
    {
        public Translation[] translations { get; set; }
    }

    public class Translation
    {
        public string text { get; set; }
        public string to { get; set; }
    }

}