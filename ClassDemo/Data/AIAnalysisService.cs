// Data/AIAnalysisService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Assignment3.Models;
using Microsoft.Extensions.Configuration;

namespace Assignment3.Data
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
            _apiKey = configuration["OpenAI:ApiKey"];
            _endpoint = configuration["OpenAI:Endpoint"];
            _model = configuration["OpenAI:Model"]; // Ensure this matches your deployment name
        }

        // Generic method to handle different types of AI responses
        public async Task<List<T>> GetChatGPTResponseAsync<T>(string prompt)
        {
            var requestBody = new
            {
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 500,
                temperature = 0.7,
                n = 1,
                stream = false,
                stop = (string)null
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUri = $"{_endpoint}/openai/deployments/{_model}/chat/completions?api-version=2023-05-15";

            var response = await _httpClient.PostAsync(requestUri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var aiMessageContent = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // Extract JSON array from AI's response
                string jsonArrayString = ExtractJsonArray(aiMessageContent);

                // Deserialize the JSON array into objects of type T
                var result = JsonSerializer.Deserialize<List<T>>(jsonArrayString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new Exception("Deserialized result is null.");
                }

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error from OpenAI: {response.ReasonPhrase}. Details: {errorContent}");
            }
        }

        // Generate meals for a specific nutrition plan using AI
        public async Task<List<Meal>> GenerateMealsFromAI(int totalDailyCalories, int proteinPercentage, int carbPercentage, int fatPercentage)
        {
            string prompt = $@"
                Generate a list of 3 meals for a day to fulfill the following nutritional requirements. Categorize each meal as breakfast, lunch, or dinner:
                - Total Daily Calories: {totalDailyCalories} kcal
                - Protein: {proteinPercentage}%
                - Carbohydrates: {carbPercentage}%
                - Fat: {fatPercentage}%

                Ensure that the types for each property are strictly as follows:
                - Name (String)
                - Description (String)
                - Calories (Int)
                - Protein (Int)
                - Carbs (Int)
                - Fat (Int)

                Format the response as a JSON array with each meal having the specified properties.
            ";

            try
            {
                return await GetChatGPTResponseAsync<Meal>(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating meals: {ex.Message}");
                throw;
            }
        }

        // Generate exercises for a specific routine using AI
        public async Task<List<Exercise>> GenerateExercisesFromAI(string routineType, float currentWeight, float goalWeight, int timeFrame)
        {
            string prompt = $@"
                Generate a list of exercises for a {routineType} routine based on the following parameters, consider the time frame, and difference between current weight and goal weight when deciding the intensity (number of exercises) of each routine:
                - Current Weight: {currentWeight} lbs
                - Goal Weight: {goalWeight} lbs
                - Time Frame: {timeFrame} weeks

                Ensure each exercise includes:
                - Name (String)
                - Description (String)
                - Sets (Int)
                - Reps (Int) < If exercise is in minutes instead of reps, return 0 for reps>
                

                Format the response as a JSON array with each exercise having the specified properties.
            ";

            try
            {
                return await GetChatGPTResponseAsync<Exercise>(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating exercises: {ex.Message}");
                throw;
            }
        }

        private string ExtractJsonArray(string responseContent)
        {
            int startIndex = responseContent.IndexOf('[');
            int endIndex = responseContent.LastIndexOf(']');

            if (startIndex == -1 || endIndex == -1)
            {
                throw new FormatException("JSON array not found in the response.");
            }

            return responseContent.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
}