using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Llhama3_test.Class
{
    public class Ollama{
        private HttpClient Client;
        private HttpRequestMessage Request;
        private OSetting Setting;
        private string prompt;
        private string error;
        private string response = "";
        public Ollama(){
            Setting = new OSetting();
            Client = new HttpClient();
        }

        public async Task<string> sendRequest(string p){
            try{
                if (!String.IsNullOrEmpty(p)) {
                    setPrompt(p);
                    
                    Request = new HttpRequestMessage(HttpMethod.Post, Setting.apiUrlName);

                    // ASSEMBLE CONTENT
                    var json = JsonSerializer.Serialize(new{prompt, Setting.model, Setting.stream});
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // ASSOCIATES THE CONTENT WITH REQUEST
                    Request.Content = content;

                    // SET HTTP CLIENT 
                    Client = new HttpClient();
                    Client.Timeout = TimeSpan.FromSeconds(Setting.timeout);

                    // SEND REQUEST
                    var r = await Client.SendAsync(Request);
                    if (r.StatusCode == HttpStatusCode.OK)
                    { 
                        // READ RESPONSE AS STRING
                        var responseBody = await r.Content.ReadAsStringAsync();

                        JObject parsedResponse = JObject.Parse(responseBody);
                        response = parsedResponse["response"].ToString();
                    } else {
                        // READ ERROR RESPONSE AS STRING
                        var errorResponse = await r.Content.ReadAsStringAsync();
                        error = $"Error: {r.StatusCode}. Details: {errorResponse}";
                    }
                } else {
                    error = "Prompt must be not null or empty";
                }
            } catch(Exception ex){
                error = ex.Message;
            }
            
            return response;
        }

        //  GETS AND SETS
        private void setPrompt(string p){
            prompt = p;
        }
        public void setApiUrlName(string url){
            Setting.apiUrlName = url;
        }
        public void setTimeout(int t){
            Setting.timeout = t;
        }
        public void setModel(string model){
            Setting.model = model;
        }
        public void setStream(bool stream){
            Setting.stream = stream;
        }
        public string getError(){
            return error;
        }
    }

    partial class OSetting {
        public string apiUrlName = "http://localhost:11434/api/generate";
        public string model = "llama3.1";
        public bool stream = false;
        public int timeout = 30;
    }
}
