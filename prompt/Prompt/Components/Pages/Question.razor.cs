using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.JSInterop;
using System;
using System.Web;
using System.Diagnostics;
using Prompt.Components.Class;

namespace Prompt.Components.Pages
{
    public partial class Question : ComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private IJSObjectReference questionModule;
        private Prompt Prompt = new Prompt();
        private Ollama Ollama;
        private OllamaApi OllamaApi;

        private List<object> responses = new List<object>();
        Stopwatch stopwatch = new Stopwatch();
        private string answer = "";
        private bool loadingAnswer = false;
        private bool formDisabled = false;
        public bool getFromLocalOllama = true; // CHANGE THIS VAR TO GET FROM AN API DESTINATION 

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
                if (prompt != null && prompt != "") {
                    // Preparing loading state
                    Prompt.text = prompt;
                    await setLoadingState();
                    
                    // Treat string to URL
                    string encodedPrompt = HttpUtility.UrlEncode(prompt);

                    // Sending promp request
                    if (getFromLocalOllama){
                        Ollama = new Ollama();
                        Ollama.setTimeout(50);
                        answer = await Ollama.sendRequest(prompt);
                    } else {
                        string url= $"http://localhost:5102/api/v1/sendMessage?prompt={encodedPrompt}"; // CHANGE THIS VAR TO YOUR END POINT API
                        
                        OllamaApi = new OllamaApi();
                        OllamaApi.setTimeout(50);
                        answer = await OllamaApi.sendRequest(prompt, url);
                    }
                    if (string.IsNullOrEmpty(answer)) {
                        answer = "Error on API.";
                    }
                    
                    // Preparing loaded state
                    await setLoadingState(false);

                    // Rec proccess time and prepering response
                    decimal time = Math.Round((decimal)(stopwatch.ElapsedMilliseconds / 1000), 0);
                    object response = new { index = responses.Count(), question = prompt, answer = answer, time = time };
                    
                    // Set response
                    await appendInResponses(response);
                    StateHasChanged();
                } else {
                    await JSRuntime.InvokeVoidAsync("console.log", "No prompt sent");
                }
            }
            catch (Exception ex)
            {
                await setLoadingState(false);
                await JSRuntime.InvokeVoidAsync("console.log", "Error: "+ex.Message);
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
}
