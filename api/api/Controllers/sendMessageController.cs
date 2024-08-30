using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Llhama3_test.Controllers
{
    [ApiController]
    [Route("api/v1/sendMessage")]
    public class sendMessageController : ControllerBase
    {
        private string answer = "";
        private readonly HttpClient _httpClient;

        public sendMessageController()
        {
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<string> ask(string prompt)
        {
            await getLlamaResponse(prompt);

            return answer;
        }
        private async Task getLlamaResponse(string prompt, string model = "llama3", bool stream = false)
        {
            // Assemble Request
            string protocol = "http://";
            string domain = "localhost";
            string port = ":11434";
            string uri = "/api/generate";
            string url = (protocol + domain + port + uri);
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            // Assemble Post
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                prompt,
                model,
                stream
            }), Encoding.UTF8, "application/json");

            // Link request and content
            request.Content = content;

            // Validate end-point sync
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Extract body 
            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseBody, options);

            // Access string response to give back
            answer = responseObject.response;
        }

        public class OllamaResponse
        {
            public string model { get; set; }
            public DateTime createdAt { get; set; }
            public string response { get; set; }
            public bool done { get; set; }
            public string doneReason { get; set; }
            public int[] context { get; set; }
        }
    }
}
