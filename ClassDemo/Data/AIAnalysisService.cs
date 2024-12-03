// Data/AIAnalysisService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClassDemo.Models;
using Microsoft.Extensions.Configuration;

namespace ClassDemo.Data
{
    public class AIAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;

        public AIAnalysisService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "9a33d791af754494aca3eaa0268011a4";
            _endpoint = "https://fall2024-oagregory-openai.openai.azure.com/";
            _model = "gpt-35-turbo";
        }

        // Generic method to handle different types of AI responses
        public async Task<List<T>> GetChatGPTResponseAsync<T>(string prompt)
        {
            var requestBody = new
            {
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 1000,
                temperature = 0.7,
                n = 1
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var apiVersion = "2023-05-15";
            var requestUri = $"{_endpoint}openai/deployments/{_model}/chat/completions?api-version={apiVersion}";

            int retryCount = 0;
            int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                var response = await _httpClient.PostAsync(requestUri, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonDocument.Parse(responseContent);

                    var aiMessageContent = jsonResponse.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    // Extract JSON array from AI's response
                    string jsonArrayString = ExtractJsonArray(aiMessageContent);

                    // Deserialize the JSON array into objects of type T
                    var items = JsonSerializer.Deserialize<List<T>>(jsonArrayString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return items;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(10);
                    await Task.Delay(retryAfter);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error from OpenAI: {response.ReasonPhrase}. Details: {errorContent}");
                }
            }

            throw new Exception("Exceeded maximum retry attempts due to rate limiting.");
        }

        private string ExtractJsonArray(string responseContent)
        {
            int startIndex = responseContent.IndexOf('[');
            int endIndex = responseContent.LastIndexOf(']');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                return responseContent.Substring(startIndex, endIndex - startIndex + 1);
            }
            else
            {
                throw new Exception("Failed to find JSON array in the AI response.");
            }
        }
    }
}
