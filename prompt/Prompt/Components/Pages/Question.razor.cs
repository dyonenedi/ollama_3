using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Prompt.Components.Pages
{
    public partial class Question : ComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private IJSObjectReference questionModule;
        private Prompt Prompt = new Prompt();
        private HttpClient client;
        private HttpRequestMessage request;
        private List<object> responses = new List<object>();
        Stopwatch stopwatch = new Stopwatch();
        private string answer = "";
        private bool loadingAnswer = false;
        private bool formDisabled = false;

        protected override async Task OnInitializedAsync()
        {
            Prompt.text = "";
        }

        [JSInvokable]
        public void callSendQuestion(string question)
        {
            sendQuestion(question).GetAwaiter();
        }
        private async Task sendQuestion(string _prompt = null)
        {
            try {
                var prompt = (_prompt != null) ? _prompt : Prompt.text;
                Prompt.text = prompt;
                await setLoadingState();

                bool getLocal = true;
                await getLlamaResponse(getLocal, prompt);

                await setLoadingState(false);

                decimal time = Math.Round((decimal)(stopwatch.ElapsedMilliseconds / 1000), 0);
                object response = new { index = responses.Count(), question = prompt, answer = answer, time = time };
                
                await appendInResponses(response);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await setLoadingState(false);
                await JSRuntime.InvokeVoidAsync("console.log", "Um erro ocorreu: "+ex.Message);
            }
        }
        private async Task getLlamaResponse(bool getLocal, string prompt, string model = "llama3", bool stream = false)
        {
            try
            {
                if (prompt != "")
                {
                    // Assemble request
                    string protocol = "http://";
                    string domain = "localhost";
                    string port = getLocal ? ":11434" : ":44358";
                    string uri = getLocal ? "/api/generate" : $"/api/v1/sendMessage?prompt={prompt}";
                    string url = protocol + domain + port + uri;
                    request = new HttpRequestMessage(getLocal ? HttpMethod.Post : HttpMethod.Get, url);

                    if (getLocal)
                    {
                        // Assemble content
                        var json = JsonSerializer.Serialize(new{prompt,model,stream});
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // Associates the content with the request
                        request.Content = content;
                    }

                    // Set a timeout to request
                    client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Make the call to the endpoint and validate if it was successful
                    var response = await client.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    { 
                        // Extract the response content in Json format
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if(getLocal)
                        {
                            // Extract Json in Object format
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseBody, options);
                            answer = responseObject.response;
                        } 
                        else
                        {
                            // Access the "response" property and convert it to a string
                            answer = JsonSerializer.Deserialize<String>(responseBody);
                        }
                    } else
                    {
                        answer = "Sorry, there is some problem in my mind. Lat's tray again.";
                    }
                }
            }
            catch (Exception e)
            {
                answer = "Sorry, there is some leg in my mind. Let's tray again.";
                setLoadingState(false);
            }
        }

        private async Task setLoadingState(bool loading = true)
        {
            if (loading)
            {
                stopwatch.Reset();
                stopwatch.Start();

                formDisabled = true;
                loadingAnswer = true;
                
                StateHasChanged();
            } else
            {
                stopwatch.Stop();

                Prompt.text = "";
                formDisabled = false;
                loadingAnswer = false;
                StateHasChanged();
            }
        }
        private async Task appendInResponses(object response)
        {
            responses.Add(response);
            if (responses.Count() > 1)
            {
                for (var i = (responses.Count() - 1); i >= 0; i--)
                {
                    if (i == 0)
                    {
                        responses[0] = response;
                    } else
                    {
                        responses[i] = responses[i-1];
                    }
                }
                
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    var thisContext = DotNetObjectReference.Create(this);
                    questionModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Question.razor.js");
                    await questionModule.InvokeVoidAsync("setFormEnterSubmit", thisContext);
                }
            } catch(Exception e){
                await JSRuntime.InvokeVoidAsync("console.log", e.Message);
            }
        }
    }

    public class Prompt
    {
        public string? text { get; set; } = "";
    }
    public class OllamaResponse
    {
        public string? model { get; set; }
        public DateTime createdAt { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
        public string doneReason { get; set; }
        public string done_Reason { get; set; }
        public int[]? context { get; set; }
    }
}
