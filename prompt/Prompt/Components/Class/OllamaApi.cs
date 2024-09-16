using System;
using System.Net;

namespace Prompt.Components.Class
{
    public class OllamaApi
    {
        private HttpClient Client;
        private HttpRequestMessage Request;
        private string response = "";
        private int timeout;
        public OllamaApi()
        {
            Client = new HttpClient();
        }

        public async Task<string> sendRequest(string prompt, string url){
            try{
                if (!String.IsNullOrEmpty(prompt)) {
                    Request = new HttpRequestMessage(HttpMethod.Get, url);

                    // SET HTTP CLIENT 
                    Client = new HttpClient();
                    Client.Timeout = TimeSpan.FromSeconds(timeout);

                    // SEND REQUEST
                    var r = await Client.SendAsync(Request);
                    if (r.StatusCode == HttpStatusCode.OK)
                    { 
                        // READ RESPONSE AS STRING IF LOCAL
                        var responseBody = await r.Content.ReadAsStringAsync();
                        response = responseBody;
                    } else {
                        // READ ERROR RESPONSE AS STRING
                        var errorResponse = await r.Content.ReadAsStringAsync();
                        response = $"Error: {r.StatusCode}. Details: {errorResponse}";
                    }
                } else {
                    response = "Prompt must be not null or empty";
                }
            } catch(Exception ex){
                response = ex.Message;
            }
            
            return response;
        }

        public void setTimeout(int t){
            timeout = t;
        }
    }
}