using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Llhama3_test.Controllers
{
    [ApiController]
    [Route("api/v1/iderisAI")]
    public class iderisAiController : ControllerBase
    {
        private string answer = "";
        private readonly HttpClient _httpClient;

        public iderisAiController()
        {
            _httpClient = new HttpClient();
        }

        [HttpGet]
        public async Task<string> ask(string prompt)
        {
            await getLlamaResponse(prompt);

            return answer;
        }
        private async Task getLlamaResponse(string prompt, string model = "llama3.1", bool stream = false)
        {
            // Monta a request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate");
            // Monta o content do Post
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                prompt,
                model,
                stream
            }), Encoding.UTF8, "application/json");

            // Associa o conteúdo a requisição
            request.Content = content;

            // Faz a chamada no endpoint e valida se deu sucesso
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Extrai o conteúdo da resposta em forma Json
            var responseBody = await response.Content.ReadAsStringAsync();
            // Extrai o Json em formato de Objeto
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseBody, options);
            // Acessar a propriedade "response" e converter para string
            answer = responseObject.Response;
        }

        public class OllamaResponse
        {
            public string Model { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Response { get; set; }
            public bool Done { get; set; }
            public string DoneReason { get; set; }
            public int[] Context { get; set; }
        }
    }
}
