using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Prompt.Components.Pages
{
    public partial class Question : ComponentBase
    {
        [Inject] private IJSRuntime jsRuntime { get; set; }
        private Prompt prompt = new Prompt();
        private EditContext PromptContext;
        private HttpClient _httpClient = new HttpClient();
        private List<String> responses = new List<string>();
        private bool loadingAnswer = false;
        protected override async Task OnInitializedAsync()
        { 
            PromptContext = new EditContext(prompt);
        }
        public void showModel(KeyboardEventArgs args)
        {
            Console.WriteLine($"Tecla pressionada: {args.Key}");
            jsRuntime.InvokeVoidAsync("console.log", prompt.Text);
        }

        private async Task OnSubmit()
        {
            Console.WriteLine("OnSubmit foi acionado.");
            // Resto do código...
        }
        public async Task SendQuestionLlama()
        {
            try {
                loadingAnswer = true;
                StateHasChanged();
                //await Task.Delay(1000);
                var _prompt = prompt.Text;
                Console.WriteLine("SendQuestionLlama foi chamado.");
                goto FIM;
                var model = "llama3.1";
                var stream = false;

                // Monta a request
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate");
                // Monta o content do Post
                var content = new StringContent(JsonSerializer.Serialize(new
                {
                    _prompt,
                    model,
                    stream
                }), Encoding.UTF8, "application/json");

                // Associa o conteúdo a requisição
                request.Content = content;

                // Faz a chamada no endpoint e valida se deu sucesso
                var response = await _httpClient.SendAsync(request);
                //response.EnsureSuccessStatusCode();

                // Extrai o conteúdo da resposta em forma Json
                var responseBody = await response.Content.ReadAsStringAsync();
                // Extrai o Json em formato de Objeto
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseBody, options);
                // Acessar a propriedade "response" e converter para string
                var _response = responseObject.Response;
                responses.Add(_response);
                prompt.Text = "";
                FIM:
                loadingAnswer = false;
            }
            catch (Exception ex)
            {
                responses.Add("Um erro ocorreu: "+ex.Message);
            }
        }

        protected async Task sendQuestionAPI()
        {
            responses.Add("Olá Dyon, como vai?");
            // Monta a request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5139/api/v1/IderisAI");
            // Monta o content do Post
            var content = new StringContent(JsonSerializer.Serialize(new{prompt.Text}), Encoding.UTF8, "application/json");

            // Associa o conteúdo a requisição
            request.Content = content;

            // Faz a chamada no endpoint e valida se deu sucesso
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Extrai o conteúdo da resposta em forma Json
            var responseBody = await response.Content.ReadAsStringAsync();

            // Extrai 
            var _response = JsonSerializer.Deserialize<String>(responseBody);
            responses.Add(_response);
        }
        protected override async Task OnParametersSetAsync()
        {
            //await jsRuntime.InvokeVoidAsync("console.log", "Prompt: " + prompt.Text);
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await jsRuntime.InvokeVoidAsync("console.log", "Prompt: " + prompt.Text);
        }
    }

    public class Prompt
    {
        public string? Text { get; set; } = "";
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
