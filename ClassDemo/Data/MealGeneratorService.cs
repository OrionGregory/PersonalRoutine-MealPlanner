// Data/MealGeneratorService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment3.Models;

namespace Assignment3.Data
{
    public class MealGeneratorService
    {
        private readonly AIAnalysisService _aiAnalysisService;

        public MealGeneratorService(AIAnalysisService aiAnalysisService)
        {
            _aiAnalysisService = aiAnalysisService;
        }

        public async Task<List<Meal>> GenerateMealsAsync(int totalDailyCalories, int proteinPercentage, int carbPercentage, int fatPercentage)
        {
            // Generate meals using AIAnalysisService
            var generatedMeals = await _aiAnalysisService.GenerateMealsFromAI(totalDailyCalories, proteinPercentage, carbPercentage, fatPercentage);
            return generatedMeals;
        }
    }
}