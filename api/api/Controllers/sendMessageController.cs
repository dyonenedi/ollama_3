using ollama_3.Class;
using Microsoft.AspNetCore.Mvc;

namespace ollama_3.Controllers
{
    [ApiController]
    [Route("api/v1/sendMessage")]
    public class sendMessageController : ControllerBase
    {
        private Ollama Ollama;
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
        private async Task getLlamaResponse(string prompt)
        {
            Ollama = new Ollama();
            Ollama.setTimeout(50);
            answer = await Ollama.sendRequest(prompt);
            if (string.IsNullOrEmpty(answer)) {
                answer = Ollama.getError();
            }
        }
    }
}
